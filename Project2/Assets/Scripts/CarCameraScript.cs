using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script to make the camera follow the car-like behaviour
// adpated from: https://answers.unity.com/questions/900069/camera-turns-to-the-front-when-reversing-car.html
public class CarCameraScript : MonoBehaviour {

	public Transform car;
	public float distance = 15.0f; //6.4f;
	public float height = 5.0f; //1.4f;
	public float rotationDamping = 3.0f;
	public float heightDamping = 2.0f;
	public float zoomRatio = 0.5f;
	public float defaultFOV = 60f;

	private Vector3 rotationVector;

	void LateUpdate(){
		float wantedAngle = rotationVector.y;
		float wantedHeight = car.position.y + height;
		float myAngle = transform.eulerAngles.y;
		float myHeight = transform.position.y;

		myAngle = Mathf.LerpAngle(myAngle, wantedAngle, rotationDamping * Time.deltaTime);
		myHeight = Mathf.Lerp(myHeight, wantedHeight, heightDamping * Time.deltaTime);

		Quaternion currentRotation = Quaternion.Euler(0, myAngle, 0);
		transform.position = car.position;
		transform.position -= currentRotation * Vector3.forward * distance;
		Vector3 temp = transform.position;
		temp.y = myHeight;
		transform.position = temp;
		transform.LookAt(car);
	}

	void FixedUpdate(){
		Vector3 localVelocity = car.InverseTransformDirection(car.GetComponent<Rigidbody>().velocity);
		if (localVelocity.z < -0.1f){
			Vector3 temp = rotationVector;
			temp.y = car.eulerAngles.y + 180;
			rotationVector = temp;
		}
		else{
			Vector3 temp = rotationVector;
			temp.y = car.eulerAngles.y;
			rotationVector = temp;
		}
		float acc = car.GetComponent<Rigidbody>().velocity.magnitude;
		GetComponent<Camera>().fieldOfView = defaultFOV + acc * zoomRatio * Time.deltaTime; 
	}
}
