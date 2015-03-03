using UnityEngine;
using System.Collections;

public class PointLightScript : MonoBehaviour {

	private Light light;


	bool intensityDown = true;
	// Use this for initialization
	void Start () {
		light = GetComponent<Light>();
	}
	
	// Update is called once per frame
	void Update () {

		if(intensityDown)
		{
			light.intensity -= 0.15f;
		} else {
			light.intensity += 0.15f;
		}
		
		if(light.intensity <= 0f)
		{
			intensityDown = false;
		} if (light.intensity >= 2.5f){
			intensityDown = true;
		}
	}
}
