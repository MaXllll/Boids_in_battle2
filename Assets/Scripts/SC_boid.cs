using UnityEngine;
using System.Collections;

public class SC_boid : MonoBehaviour {
	
	[SerializeField]
	public Transform _T_boid;
	[SerializeField]
	private Transform _T_graphic;
	[SerializeField]
	private Animator _animator;

	public SC_boids_team _boids_team;
	
	public SC_boid _boid_target;

	private int _i_hp = 20;
	private float _f_attack_delay = 1.5f;
	private bool _b_attack_is_reloaded = true;
	private float _f_timer_attack = 0;
	[SerializeField]
	private bool  _b_can_launch = false;
	private float _f_launch_delay = 1.5f;
	private bool _b_launch_is_reloaded = true;
	private float _f_timer_launch = 0;
	[HideInInspector]
	public bool _b_is_dead = false;

	//[HideInInspector]
	public int _i_nb_agressors = 0;

	public float _f_max_speed = 10;
	[SerializeField]
	private float _f_acceleration = 20;

	[SerializeField]
	private int _i_nb_random_attack = 0;

	[HideInInspector]
	public Vector3 _V3_target = Vector3.zero;
	private Vector3 _V3_velocity = Vector3.zero;

	[HideInInspector]
	public Vector3 _V3_thread_position;
	[HideInInspector]
	public Vector3 _V3_thread_velocity;
	[HideInInspector]
	public bool _V3_thread_have_boid_target;


	public void UpdateThreadInfo()
	{
		_V3_thread_position = _T_boid.position;
		_V3_thread_velocity = _V3_velocity;
		_V3_thread_have_boid_target = (_boid_target != null);
	}


	public void Update()
	{
		if (_b_is_dead)
			return;

		Vector3 V3_velocity_target = Vector3.zero;

		if (_boids_team._b_is_fleeing)
		{
			V3_velocity_target = -_boids_team._V3_destination_direction;
			V3_velocity_target += _V3_target;
			V3_velocity_target.Normalize();
		}
		else if (_boid_target != null)
		{
			_T_boid.position += _V3_target.normalized * Time.deltaTime * 0.5f;
			V3_velocity_target = _boid_target._T_boid.position - _T_boid.position;
			_T_graphic.rotation = Quaternion.Lerp(_T_graphic.rotation, Quaternion.LookRotation(V3_velocity_target), Time.deltaTime * 4);

			if (V3_velocity_target.sqrMagnitude < 16)
				V3_velocity_target = Vector3.zero;
			else
				V3_velocity_target.Normalize();

			if (_b_can_launch && _b_attack_is_reloaded && V3_velocity_target != Vector3.zero)
			{
				
				//TODO: Valentin, do your shit here !
				
				_b_launch_is_reloaded = false;
				_f_timer_launch = _f_launch_delay;
			}
			else if (_b_attack_is_reloaded && V3_velocity_target == Vector3.zero)
			{
				StartCoroutine(PlayAttackAnim());

				int i_damage = Random.Range(1,5);
				bool b_target_is_dead = _boid_target.Damage(i_damage);
				if (b_target_is_dead)
					_boid_target = null;

				_b_attack_is_reloaded = false;
				_f_timer_attack = _f_attack_delay;
			}
		}
		else if (_boids_team != null)
		{
			V3_velocity_target = _boids_team._V3_destination_direction + (_boids_team._team_enemy._V3_center_of_mass - _T_boid.position);
			if (V3_velocity_target.sqrMagnitude < 100)
				V3_velocity_target = Vector3.zero;
			else
				V3_velocity_target.Normalize();

			if (V3_velocity_target != Vector3.zero)
			{
				V3_velocity_target += _V3_target;
				V3_velocity_target.Normalize();
			}
		}

		Vector3 V3_avoid_obstacle = AvoidObstacles(V3_velocity_target);
		if (V3_avoid_obstacle != Vector3.zero)
			V3_velocity_target = V3_avoid_obstacle;

		_V3_velocity = Vector3.MoveTowards(_V3_velocity, V3_velocity_target * _f_max_speed, _f_acceleration * Time.deltaTime);
		_V3_velocity.y = 0f;

		if (_V3_velocity != Vector3.zero)
		{
			_T_graphic.rotation = Quaternion.Lerp(_T_graphic.rotation, Quaternion.LookRotation(_V3_velocity), Time.deltaTime * 4);
			_T_boid.position += _V3_velocity * Time.deltaTime;
			if (_animator != null)
				_animator.SetBool("Run", true);
		}
		else if (_animator != null)
			_animator.SetBool("Run", false);

		if (!_b_attack_is_reloaded)
		{
			_f_timer_attack -= Time.deltaTime;
			if (_f_timer_attack <= 0)
			{
				_b_attack_is_reloaded = true;
				_f_timer_attack = 0;
			}
		}

		if (!_b_launch_is_reloaded)
		{
			_f_timer_launch -= Time.deltaTime;
			if (_f_timer_launch <= 0)
			{
				_b_launch_is_reloaded = true;
				_f_timer_launch = 0;
			}
		}
	}


	public bool Damage(int i_damage)
	{
		if (_b_is_dead)
			return true;

		_i_hp -= i_damage;

		if (_i_hp <= 0)
		{
			_b_is_dead = true;
			_animator.SetBool("Die", true);
			_boids_team._i_nb_boid_alive--;
			if (_boid_target != null)
			{
				_boid_target._i_nb_agressors--;
				_boid_target = null;
			}

			/*if (_T_graphic != null)
				Destroy(_T_graphic.gameObject);*/
			return true;
		}
		return false;
	}

	private IEnumerator PlayAttackAnim()
	{
		if (_i_nb_random_attack > 0)
		{
			if (_animator != null)
				_animator.SetInteger("Attack", Random.Range(1, _i_nb_random_attack + 1));
			yield return null;
			if (_animator != null)
				_animator.SetInteger("Attack", 0);
		}
		else
		{
			if (_animator != null)
				_animator.SetBool("Attack", true);
			yield return null;
			if (_animator != null)
				_animator.SetBool("Attack", false);
		}
	}

	private Vector3 AvoidObstacles(Vector3 V3_tmp_target)
	{
		RaycastHit hit;

		if (_V3_velocity != Vector3.zero)
		{
			Vector3 V3_direction = _V3_velocity.normalized;
			if (Physics.Raycast(_T_boid.position, V3_direction, out hit, 15))
			{	
				Vector3 V3_move;
				Vector3 V3_projection_forward = ProjectVectorOnPlane(hit.normal, V3_direction);
				V3_projection_forward.Normalize();
				
				if (Vector3.Dot(hit.normal, V3_tmp_target) > 0)
				{
					Vector3 V3_projection_normal = ProjectVectorOnPlane(-V3_direction, hit.normal);
					
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
		}
		
		return Vector3.zero;
	}

	private Vector3 ProjectVectorOnPlane(Vector3 V3_normal, Vector3 V3_to_project){
		
		return V3_to_project - (Vector3.Dot(V3_to_project, V3_normal) * V3_normal);
	}
}
