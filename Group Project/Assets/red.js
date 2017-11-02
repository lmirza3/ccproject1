#pragma strict
private var red : Color = Color(0.9, 0, 0, 0.9);
var newMaterial : Material;
var target : Collider;
private var counter : int = 0;
var mySound : AudioClip;

function OnTriggerEnter(cubeTrigger : Collider)
{
if (cubeTrigger == target)
{

GetComponent.<Renderer>().material.color = red;
newMaterial.color = red;

GetComponent.<AudioSource>().PlayOneShot(mySound);
counter = counter + 1;
print("Collided: " + counter + " times!");
}
}


