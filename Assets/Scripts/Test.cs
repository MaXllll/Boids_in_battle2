using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

	private Animator anim;

	// Use this for initialization
	void Start () {
		anim = this.GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void FixedUpdate (){

		anim.SetInteger("Attack", 0);
		anim.SetBool("Walk", false);
		anim.SetBool("Run", false);
		anim.SetBool("Die", false);

		if(Input.GetKey(KeyCode.Z))
		{
			anim.SetBool("Walk", true);
		}

		if (Input.GetKey (KeyCode.A))
		{
			int rand = Random.Range(1,5);
			anim.SetInteger("Attack", rand);
		}

		if (Input.GetKey (KeyCode.R))
		{
			anim.SetBool("Run", true);
		}

		if (Input.GetKey (KeyCode.D))
		{
			anim.SetBool("Die", true);
		}

	}
}
