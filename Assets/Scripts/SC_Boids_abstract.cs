using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class SC_Boids_abstract : MonoBehaviour {

	[SerializeField]
	public Transform _T_boid;
	[SerializeField]
	public Transform _T_graphic;

	[SerializeField]
	public List<Transform> _A_T_Boid= new List<Transform>();
	[SerializeField]
	public List<Transform> _A_T_graphic= new List<Transform>();

	[SerializeField]
	public Animator _animator;
	
	public SC_boids_team _boids_team;
	
	public SC_Boids_abstract _boid_target;
	
	public int _i_hp = 20;
	public float _f_attack_delay = 1.5f;
	public bool _b_attack_is_reloaded = true;
	public float _f_timer_attack = 0;
	[HideInInspector]
	public bool _b_is_dead = false;
	
	[SerializeField]
	public float _f_max_speed = 1;
	[SerializeField]
	public float _f_acceleration = 5;
	
	[HideInInspector]
	public Vector3 _V3_target = Vector3.zero;
	public Vector3 _V3_velocity = Vector3.zero;
	
	[HideInInspector]
	public Vector3 _V3_thread_position;
	[HideInInspector]
	public Vector3 _V3_thread_velocity;
	[HideInInspector]
	public bool _V3_thread_have_boid_target;

	public abstract void UpdateThreadInfo();
	
	public abstract void Update();
	
	public abstract bool Damage(int i_damage);

	
	public abstract IEnumerator PlayAttackAnim();
}
