using UnityEngine;
using System.Collections;
using System.Threading;

public class SC_boids_3D : MonoBehaviour {

	[SerializeField]
	private int _i_nb_boids = 50;
	[SerializeField]
	private GameObject _Prefab_boid;
	[SerializeField]
	private Transform _T_root_boids;
	[SerializeField]
	private float _f_speed = 8;
	
	[SerializeField]
	private float _f_factor_smooth_rotate = 1;
	
	[SerializeField]
	private float _f_factor_inertia = 1;
	
	[SerializeField]
	private float _f_distance_separation = 3;
	[SerializeField]
	private float _f_factor_separation = 3;
	
	[SerializeField]
	private float _f_distance_alignment = 10;
	[SerializeField]
	private float _f_factor_alignment = 2;
	
	[SerializeField]
	private float _f_distance_aggregation = 10;
	[SerializeField]
	private float _f_factor_aggregation = 2;
	
	[SerializeField]
	private float _f_distance_dispersion = 50;
	[SerializeField]
	private float _f_factor_dispersion = 25;
	
	[SerializeField]
	private float _f_radius_holding_area = 75;
	[SerializeField]
	private Vector3 _V3_center_holding_area = Vector3.zero;
	[SerializeField]
	private float _f_factor_holding_area = 10;

	[SerializeField]
	private bool _b_avoid_obstacles = true;
	[SerializeField]
	private bool _b_smart_avoid_obstacles = true;
	[SerializeField]
	private float _f_distance_avoid_obstacles = 10;
	[SerializeField]
	private int _i_nb_group_raycast = 4;
	private int _i_nb_group_raycast_index = 0;

	[SerializeField]
	private float _f_factor_destination = 2;
	
	private Transform[] _T_boids;
	private Vector3[] _V3_boids_target;
	private Vector3[] _V3_boids_position;
	private Vector3[] _V3_boids_forward;
	[SerializeField]
	private Transform[] _T_predators;
	[SerializeField]
	private Transform _T_goal;
	private Vector3[] _V3_predators_position;
	private bool[] _b_is_predators_active;
	private Vector3 _V3_goal_position;
	private bool _b_is_goal_active;

	[SerializeField]
	private bool _b_draw_ray = false;

	[SerializeField]
	private int _i_nb_thread = 1;
	private bool[] _b_is_calcul_target_finish;


	
	void Start()
	{
		_T_boids = new Transform[_i_nb_boids];
		_V3_boids_target = new Vector3[_i_nb_boids];
		_V3_boids_position = new Vector3[_i_nb_boids];
		_V3_boids_forward = new Vector3[_i_nb_boids];
		for (int i = 0; i < _i_nb_boids; ++i)
		{
			GameObject GO_tmp = Instantiate(_Prefab_boid, new Vector3(Random.value * 100 - 50, Random.value * 100 - 50, Random.value * 100 - 50), Quaternion.Euler(new Vector3(Random.value * 360, Random.value * 360, Random.value * 360))) as GameObject;
			_T_boids[i] = GO_tmp.transform;
			_T_boids[i].parent = _T_root_boids;
			_V3_boids_target[i] = _T_boids[i].forward;
		}

		_V3_predators_position = new Vector3[_T_predators.Length];
		_b_is_predators_active = new bool[_T_predators.Length];

		_b_is_calcul_target_finish = new bool[_i_nb_thread];
		for(int i = 0; i < _i_nb_thread; ++i)
		{
			_b_is_calcul_target_finish[i] = true;
		}
	}
	
	
	void Update()
	{
		for(int i = 0; i < _i_nb_thread; ++i)
		{
			if (_b_is_calcul_target_finish[i])
			{
				for (int j = 0; j < _i_nb_boids; ++j)
				{
					_V3_boids_position[j] = _T_boids[j].position;
					_V3_boids_forward[j] = _T_boids[j].forward;
				}
				for (int j = 0; j < _T_predators.Length; ++j)
				{
					_V3_predators_position[j] = _T_predators[j].position;
				}
				_b_is_goal_active = _T_goal != null && _T_goal.gameObject.activeInHierarchy;
				_V3_goal_position = _T_goal.position;

				_b_is_calcul_target_finish[i] = false;
				Thread thread = new Thread(CaclulateBoidsTargets);
				thread.Start((object)i);
			}
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			Vector3 V3_position = Vector3.zero;
			for (int i = 0; i < _i_nb_boids; ++i)
			{
				V3_position += _T_boids[i].position + _T_boids[i].forward * _f_speed * _f_factor_smooth_rotate;
			}
			V3_position /= _i_nb_boids;
			_T_predators[0].position = V3_position;
		}
		
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			_T_predators[0].position = new Vector3(0, 1000, 0);
		}

		if (_b_avoid_obstacles)
		{
			int i_start_boid = _i_nb_group_raycast_index * (_i_nb_boids / _i_nb_group_raycast);
			int i_end_boid;
			if (_i_nb_group_raycast_index == _i_nb_group_raycast - 1)
				i_end_boid = _i_nb_boids;
			else
				i_end_boid = (_i_nb_group_raycast_index + 1) * (_i_nb_boids / _i_nb_group_raycast);

			for (int i = i_start_boid; i < i_end_boid; ++i)
			{
				Vector3 V3_avoid_obstacle = AvoidObstacles(i, _V3_boids_target[i]);
				if (V3_avoid_obstacle != Vector3.zero)
					_V3_boids_target[i] = V3_avoid_obstacle;
			}

			++_i_nb_group_raycast_index;
			if (_i_nb_group_raycast_index == _i_nb_group_raycast)
				_i_nb_group_raycast_index = 0;
		}

		for (int i = 0; i < _i_nb_boids; ++i)
		{
			_T_boids[i].rotation = Quaternion.Lerp(_T_boids[i].rotation, Quaternion.LookRotation(_V3_boids_target[i]), Time.deltaTime * _f_factor_smooth_rotate);
			_T_boids[i].position += _T_boids[i].forward * _f_speed * Time.deltaTime;
			
			if (_b_draw_ray)
			{
				Debug.DrawRay(_T_boids[i].position, _T_boids[i].forward * 3f, Color.green);
				Debug.DrawRay(_T_boids[i].position, _V3_boids_target[i] * 3f, Color.red);
			}
		}
	}


	private void CaclulateBoidsTargets(object o_thread)
	{
		int i_thread = (int) o_thread;
		int i_start_boid = i_thread * (_i_nb_boids / _i_nb_thread);
		int i_end_boid;
		if (i_thread == _i_nb_thread - 1)
			i_end_boid = _i_nb_boids;
		else
			i_end_boid = (i_thread + 1) * (_i_nb_boids / _i_nb_thread);

		for (int i = i_start_boid; i < i_end_boid; ++i)
		{
			Vector3 V3_target_separation = Vector3.zero;
			Vector3 V3_target_alignment = Vector3.zero;
			Vector3 V3_target_aggregation = Vector3.zero;
			int i_nb_near_boids_separation = 0;
			int i_nb_near_boids_alignment = 0;
			int i_nb_near_boids_aggregation = 0;
			for (int j = 0; j < _i_nb_boids; ++j)
			{
				float f_distance = Vector3.Distance(_V3_boids_position[i], _V3_boids_position[j]);
				if (f_distance > 0 && f_distance < _f_distance_separation)
				{
					++i_nb_near_boids_separation;
					V3_target_separation += (_V3_boids_position[j] - _V3_boids_position[i]).normalized * (f_distance - _f_distance_separation) / _f_distance_separation;
				}
				
				if (f_distance > 0 && f_distance < _f_distance_alignment)
				{
					++i_nb_near_boids_alignment;
					V3_target_alignment += _V3_boids_forward[j] * (f_distance - _f_distance_separation) / _f_distance_separation;
				}
				
				if (f_distance > 0 && f_distance < _f_distance_aggregation)
				{
					++i_nb_near_boids_aggregation;
					V3_target_aggregation += _V3_boids_position[i];
				}
			}
			
			if (i_nb_near_boids_separation > 0)
				V3_target_separation /= i_nb_near_boids_separation;
			V3_target_separation.Normalize();

			if (i_nb_near_boids_alignment > 0)
				V3_target_alignment /= i_nb_near_boids_alignment;
			V3_target_alignment.Normalize();
			
			if (i_nb_near_boids_aggregation > 0)
				V3_target_aggregation /= i_nb_near_boids_aggregation;
			float f_distance_factor = Vector3.Distance(_V3_boids_position[i], V3_target_aggregation) / _f_distance_aggregation;
			V3_target_aggregation -= _V3_boids_position[i];
			V3_target_aggregation.Normalize();
			V3_target_aggregation *= f_distance_factor;
			
			
			Vector3 V3_target = _V3_boids_forward[i] * _f_factor_inertia;
			V3_target += V3_target_separation * _f_factor_separation;
			V3_target += V3_target_alignment * _f_factor_alignment;
			V3_target += V3_target_aggregation * _f_factor_aggregation;
			V3_target += Dispersion(i) * _f_factor_dispersion;
			V3_target += HoldingArea(i) * _f_factor_holding_area;
			V3_target += Destination(i) * _f_factor_destination;
			
			V3_target.Normalize();

			_V3_boids_target[i] = V3_target;
		}

		_b_is_calcul_target_finish[i_thread] = true;
	}


	private Vector3 Dispersion(int i_boid)
	{
		Vector3 V3_move = Vector3.zero;
		int i_nb_near_predators = 0;
		for (int i = 0; i < _T_predators.Length; ++i)
		{
			float f_distance = Vector3.Distance(_V3_boids_position[i_boid], _V3_predators_position[i]);
			if (f_distance > 0 && f_distance < _f_distance_dispersion)
			{
				++i_nb_near_predators;
				V3_move += (_V3_predators_position[i] - _V3_boids_position[i_boid]).normalized * (f_distance - _f_distance_dispersion) / _f_distance_dispersion;
			}
		}
		if (i_nb_near_predators > 0)
		{
			V3_move /= i_nb_near_predators;
		}
		
		return V3_move;
	}
	
	
	private Vector3 HoldingArea(int i_boid)
	{
		float f_distance = Vector3.Distance(_V3_boids_position[i_boid], _V3_center_holding_area);
		if (f_distance > _f_radius_holding_area)
		{
			return (_V3_center_holding_area - _V3_boids_position[i_boid]).normalized * (f_distance - _f_radius_holding_area) / _f_radius_holding_area;
		}
		
		return Vector3.zero;
	}
	
	
	private Vector3 Destination(int i_boid)
	{
		if (_b_is_goal_active)
		{
			Vector3 V3_move = _V3_goal_position - _V3_boids_position[i_boid];
			V3_move.Normalize();
			
			return V3_move;
		}
		
		return Vector3.zero;
	}
	
	
	private Vector3 AvoidObstacles(int i_boid, Vector3 V3_tmp_target)
	{
		RaycastHit hit;
		if (_b_smart_avoid_obstacles)
		{
			if (Physics.Raycast(_T_boids[i_boid].position, _T_boids[i_boid].forward, out hit, 2f))
			{
				_T_boids[i_boid].rotation = Quaternion.Lerp(_T_boids[i_boid].rotation, Quaternion.LookRotation(hit.normal), Time.deltaTime * _f_factor_smooth_rotate);
				return hit.normal;
			}
		}

		if (Physics.Raycast(_T_boids[i_boid].position, _T_boids[i_boid].forward, out hit, _f_distance_avoid_obstacles))
		{
//			if (Vector3.Dot(hit.normal, V3_tmp_target) > 0)
//			{
//				return Vector3.zero;
//			}
//			
			//			Vector3 V3_move = _V3_boids_forward[i_boid] - Vector3.Dot(_V3_boids_forward[i_boid], hit.normal) * hit.normal + hit.normal * 0.25f;
//			V3_move.Normalize();
//
//			return V3_move;

			Vector3 V3_move;
			Vector3 V3_projection_forward = ProjectVectorOnPlane(hit.normal, _T_boids[i_boid].forward);
			V3_projection_forward.Normalize();
			
			if (Vector3.Dot(hit.normal, V3_tmp_target) > 0)
			{
				Vector3 V3_projection_normal = ProjectVectorOnPlane(-_T_boids[i_boid].forward, hit.normal);
				
				if (Vector3.Dot(V3_projection_normal, V3_tmp_target) > 0)
				{
					return Vector3.zero;
				}
				else
				{
					Vector3 V3_projection_target = ProjectVectorOnPlane(V3_projection_forward, V3_tmp_target);
					V3_move = V3_projection_target.normalized;
					V3_move += hit.normal;
					V3_move.Normalize();
					return V3_move;
				}
			}
			else
			{
				Vector3 V3_projection_target = ProjectVectorOnPlane(hit.normal, V3_tmp_target);
				V3_projection_target.Normalize();
				
				if (Vector3.Dot(V3_projection_forward, V3_tmp_target) > 0)
				{
					V3_move = V3_projection_target + hit.normal;
					V3_move.Normalize();
					
					return V3_move;
				}
				else
				{
					V3_move = ProjectVectorOnPlane(V3_projection_forward, V3_projection_target).normalized;
					V3_move += hit.normal;
					V3_move.Normalize();
					
					return V3_move;
					
				}
			}
		}

		if (_b_smart_avoid_obstacles)
		{
			float f_ray_target_distance = _f_distance_avoid_obstacles * (115 - Vector3.Angle(_T_boids[i_boid].forward, V3_tmp_target)) / 115;
			f_ray_target_distance = Mathf.Max(f_ray_target_distance, 0.1f);
			if (Physics.Raycast(_T_boids[i_boid].position, V3_tmp_target, out hit, f_ray_target_distance))
			{
				Vector3 V3_move = V3_tmp_target - Vector3.Dot(V3_tmp_target, hit.normal) * hit.normal + hit.normal * 0.25f;
				V3_move.Normalize();
				
				return V3_move;
			}
		}
		
		return Vector3.zero;
	}


	private Vector3 ProjectVectorOnPlane(Vector3 V3_normal, Vector3 V3_to_project){
		
		return V3_to_project - (Vector3.Dot(V3_to_project, V3_normal) * V3_normal);
	}
}
