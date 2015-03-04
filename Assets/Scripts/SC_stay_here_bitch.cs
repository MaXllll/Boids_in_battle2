using UnityEngine;
using System.Collections;

public class SC_stay_here_bitch : MonoBehaviour {

	private Transform _T_this;
	private Vector3 _V3_start_position;


	void Start()
	{
		_T_this = transform;
		_V3_start_position = _T_this.localPosition;
	}
	

	void Update()
	{
		_T_this.localPosition = _V3_start_position;
	}
}
