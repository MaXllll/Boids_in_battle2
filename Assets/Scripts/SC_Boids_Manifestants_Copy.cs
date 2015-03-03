using UnityEngine;
using System.Collections;

public class SC_Boids_Manifestants_Copy : SC_Boids_abstract {

	public Transform manif;

	void Start() {
		int rand = Random.Range(0,6);
		manif = (Transform)Instantiate(_A_T_Boid[rand], transform.position, transform.rotation) as Transform;
		manif.SetParent(transform);
		
		_animator = manif.GetComponent<Animator>();
		_T_boid = manif;
		_T_graphic = manif;
	}

	public override void UpdateThreadInfo()
	{
		_V3_thread_position = _T_boid.position;
		_V3_thread_velocity = _V3_velocity;
		_V3_thread_have_boid_target = (_boid_target != null);
	}
	
	
	public override void Update()
	{
		if (_b_is_dead)
			return;
		
		Vector3 V3_velocity_target;
		
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
		else
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
	
	
	public override bool Damage(int i_damage)
	{
		_i_hp -= i_damage;
		
		if (_i_hp <= 0)
		{
			_b_is_dead = true;
			if (_T_graphic != null)
				//Destroy(_T_graphic.gameObject);
			return true;
		}
		return false;
	}
	
	public override IEnumerator PlayAttackAnim()
	{
		int rand = Random.Range(1,5);
		if (_animator != null)
		    _animator.SetInteger("Attack", rand);
		yield return null;
		if (_animator != null)
		{

		}
			//_animator.SetBool("isTapper", false);
	}
}
