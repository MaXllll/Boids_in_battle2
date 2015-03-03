using UnityEngine;
using System.Collections;

public class SC_rotate : MonoBehaviour {

	[SerializeField]
	private float _f_speed = 10;
	private Transform _T_root;


	void Start()
	{
		_T_root = transform;
	}

	void Update()
	{
		_T_root.Rotate(0f, _f_speed * Time.deltaTime, 0f, Space.World);
	}
}
