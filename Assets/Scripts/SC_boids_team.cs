using UnityEngine;
using System.Collections;
using System.Threading;

public class SC_boids_team : MonoBehaviour {

	[SerializeField]
	private int _i_nb_boids;
	[SerializeField]
	private GameObject[] _Prefab_boid;
	private SC_boid[] _boids;

	[SerializeField]
	private SC_boids_team _team_enemy;

	[SerializeField]
	private float _f_x_range = 20;
	[SerializeField]
	private float _f_y_range = 50;

	[SerializeField]
	private float _f_distance_aggro = 5;

	[SerializeField]
	private float _f_distance_separation = 3;
	[SerializeField]
	private float _f_factor_separation = 3;
	
	[SerializeField]
	private float _f_distance_alignment = 6;
	[SerializeField]
	private float _f_factor_alignment = 2;
	
	[SerializeField]
	private float _f_distance_aggregation = 9;
	[SerializeField]
	private float _f_factor_aggregation = 2;

	[HideInInspector]
	public Vector3 _V3_destination = Vector3.zero;
	private Vector3 _V3_center_of_mass = Vector3.zero;
	public Vector3 _V3_destination_direction { get { return _team_enemy._V3_center_of_mass - _V3_center_of_mass; } }

	private bool _b_is_calcul_target_finish = true;


	void Start()
	{
		_boids = new SC_boid[_i_nb_boids];
		for(int i = 0; i < _i_nb_boids; ++i)
		{
			GameObject GO_tmp = Instantiate(_Prefab_boid[Random.Range(0, _Prefab_boid.Length)], Vector3.zero, Quaternion.Euler(new Vector3(0f, Random.value * 360, 0f))) as GameObject;
			_boids[i] = GO_tmp.GetComponent<SC_boid>();
			_boids[i]._boids_team = this;
			_boids[i].transform.parent = transform;
			_boids[i].transform.localPosition = new Vector3(Random.value * _f_x_range - _f_x_range * 0.5f, 0f, Random.value * _f_y_range - _f_y_range * 0.5f);
		}
	}

	void Update()
	{
		if (_b_is_calcul_target_finish)
		{
			Vector3 V3_center_of_mass = Vector3.zero;
			int i_nb_boids_actif = 0;
			for(int i = 0; i < _boids.Length; ++i)
			{
				if (!_boids[i]._b_is_dead && _boids[i]._boid_target == null)
				{
					V3_center_of_mass += _boids[i]._T_boid.position;
					i_nb_boids_actif ++;
				}
				_boids[i].UpdateThreadInfo();
			}
			_V3_center_of_mass = V3_center_of_mass / i_nb_boids_actif;

			_b_is_calcul_target_finish = false;
			Thread thread = new Thread(UpdateBoidsTarget);
			thread.Start();
		}
	}

	private void UpdateBoidsTarget()
	{
		for (int i = 0; i < _boids.Length; ++i)
		{
			if (!_boids[i]._V3_thread_have_boid_target && !_boids[i]._b_is_dead)
			{
				for (int j = 0; j < _team_enemy._i_nb_boids; ++j)
				{
					if (!_team_enemy._boids[j]._V3_thread_have_boid_target && !_team_enemy._boids[j]._b_is_dead)
					{
						float f_distance = Vector3.Distance(_boids[i]._V3_thread_position, _team_enemy._boids[j]._V3_thread_position);
						if (f_distance < _f_distance_aggro)
						{
							_boids[i]._boid_target = _team_enemy._boids[j];
							_team_enemy._boids[j]._boid_target = _boids[i];

							_boids[i]._V3_thread_have_boid_target = true;
							_team_enemy._boids[j]._V3_thread_have_boid_target = true;
						}
					}
				}

				if (!_boids[i]._V3_thread_have_boid_target)
				{
					Vector3 V3_target_separation = Vector3.zero;
					Vector3 V3_target_alignment = Vector3.zero;
					Vector3 V3_target_aggregation = Vector3.zero;

					int i_nb_near_boids_separation = 0;
					int i_nb_near_boids_alignment = 0;
					int i_nb_near_boids_aggregation = 0;

					for (int j = 0; j < _i_nb_boids; ++j)
					{
						float f_distance = Vector3.Distance(_boids[i]._V3_thread_position, _boids[j]._V3_thread_position);


						if (f_distance > 0 && f_distance < _f_distance_separation)
						{
							++i_nb_near_boids_separation;
							V3_target_separation += (_boids[j]._V3_thread_position - _boids[i]._V3_thread_position).normalized * (f_distance - _f_distance_separation) / _f_distance_separation;
						}
						
						if (f_distance > 0 && f_distance < _f_distance_alignment)
						{
							++i_nb_near_boids_alignment;
							V3_target_alignment += _boids[j]._V3_thread_velocity.normalized * (f_distance - _f_distance_separation) / _f_distance_separation;
						}
						
						if (f_distance > 0 && f_distance < _f_distance_aggregation)
						{
							++i_nb_near_boids_aggregation;
							V3_target_aggregation += (_boids[i]._V3_thread_position - _boids[j]._V3_thread_position).normalized * (f_distance - _f_distance_aggregation) / _f_distance_aggregation;
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
					float f_distance_factor = Vector3.Distance(_boids[i]._V3_thread_position, V3_target_aggregation) / _f_distance_aggregation;
					V3_target_aggregation -= _boids[i]._V3_thread_position;
					V3_target_aggregation.Normalize();
					V3_target_aggregation *= f_distance_factor;



					Vector3 V3_target = Vector3.zero;
					V3_target += V3_target_separation * _f_factor_separation;
					V3_target += V3_target_alignment * _f_factor_alignment;
					V3_target += V3_target_aggregation * _f_factor_aggregation;

					if (V3_target.magnitude < 0.4f)
						V3_target = Vector3.zero;
					else
						V3_target.Normalize();
					_boids[i]._V3_target = V3_target;
				}
			}
		}

		_b_is_calcul_target_finish = true;
	}
}
