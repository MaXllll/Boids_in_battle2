using UnityEngine;
using System.Collections;

public class ControlerScript : MonoBehaviour {


	private Animator anim;
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		anim.SetBool("isAdvancing", false);	
		anim.SetBool("isStriking", false);
		anim.SetBool("isTapper",false);
		anim.SetBool("isRunning",false);
		anim.SetBool("isBackWard",false);

		if(Input.GetKey(KeyCode.UpArrow))
		{
			anim.SetBool("isAdvancing", true);	
		}

		if(Input.GetKey(KeyCode.Space))
		{
			anim.SetBool("isStriking", true);
		}
		
		if(Input.GetKey(KeyCode.LeftControl))
		{
			anim.SetBool("isTapper",true);
		}

		if(Input.GetKey(KeyCode.Z))
		{
			anim.SetBool("isRunning",true);
		}

		if(Input.GetKey(KeyCode.S))
		{
			Debug.Log("LOL");
			anim.SetBool("isBackWard",true);
		}
	}
}
