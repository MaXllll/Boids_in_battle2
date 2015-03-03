using UnityEngine;
using System.Collections;

public class SC_mouse_control : MonoBehaviour {

	[SerializeField]
	private Camera _camera;
	[SerializeField]
	private LayerMask _layer_mask;
	[SerializeField]
	private SC_boids_team _boids_team;
	

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit _hit;
			if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out _hit, 100, _layer_mask))
			{
				_boids_team._V3_destination = _hit.point;
			}
		}
	}
}
