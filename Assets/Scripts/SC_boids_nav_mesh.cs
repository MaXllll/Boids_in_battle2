using UnityEngine;
using System.Collections;

public class SC_boids_nav_mesh : MonoBehaviour {
	
	[SerializeField]
	private int _i_nb_boids = 1000;
	[SerializeField]
	private GameObject _Prefab_boid;
	[SerializeField]
	private Transform _T_root_boids;

	private NavMeshAgent[] _nav_mesh_agents;

	[SerializeField]
	private Camera _camera;
	[SerializeField]
	private LayerMask _layer_mask;


	void Start()
	{
		_nav_mesh_agents = new NavMeshAgent[_i_nb_boids];
		for (int i = 0; i < _i_nb_boids; ++i)
		{
			GameObject GO_tmp = Instantiate(_Prefab_boid, new Vector3(Random.value * 100 - 50, 0.5f, Random.value * 100 - 50), Quaternion.Euler(new Vector3(0f, Random.value * 360, 0f))) as GameObject;
			GO_tmp.transform.parent = _T_root_boids;
			_nav_mesh_agents[i] = GO_tmp.GetComponent<NavMeshAgent>();
		}
	}
	
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit _hit;
			if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out _hit, 100, _layer_mask))
			{
				for (int i = 0; i < _nav_mesh_agents.Length; ++i)
				{
					_nav_mesh_agents[i].destination = _hit.point;
				}
			}
		}
	}
}
