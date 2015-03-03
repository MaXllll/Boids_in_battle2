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
	[HideInInspector]
	public bool _b_is_dead = false;

	[SerializeField]
	private float _f_max_speed = 10;
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

		if (_boid_target != null)
		{
			V3_velocity_target = _boid_target._T_boid.position - _T_boid.position;
			_T_graphic.rotation = Quaternion.Lerp(_T_graphic.rotation, Quaternion.LookRotation(V3_velocity_target), Time.deltaTime * 4);

			if (V3_velocity_target.magnitude < 4)
				V3_velocity_target = Vector3.zero;
			else
				V3_velocity_target.Normalize();
			
			if (_b_attack_is_reloaded)
			{
				if (V3_velocity_target.magnitude < 4)
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
			else
			{
				_f_timer_attack -= Time.deltaTime;
				if (_f_timer_attack <= 0)
				{
					_b_attack_is_reloaded = true;
					_f_timer_attack = 0;
				}
			}
		}
		else if (_boids_team != null)
		{
			V3_velocity_target = _boids_team._V3_destination_direction;
			if (V3_velocity_target.magnitude < 4)
				V3_velocity_target = Vector3.zero;
			else
				V3_velocity_target.Normalize();

			if (V3_velocity_target != Vector3.zero)
			{
				V3_velocity_target += _V3_target;
				V3_velocity_target.Normalize();
			}
		}

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
	}


	public bool Damage(int i_damage)
	{
		_i_hp -= i_damage;

		if (_i_hp <= 0)
		{
			_b_is_dead = true;
			if (_T_graphic != null)
				Destroy(_T_graphic.gameObject);
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
}
