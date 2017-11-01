#pragma strict
private var orange : Color = Color(0.8, 0.4, 0.0, 0.7);
var newMaterial : Material;
var target : Collider;
private var counter : int = 0;
var mySound : AudioClip;

function OnTriggerEnter(cubeTrigger : Collider)
{
if (cubeTrigger == target)
{


GetComponent.<AudioSource>().PlayOneShot(mySound);
counter = counter + 1;
print("Collided: " + counter + " times!");
}
}



