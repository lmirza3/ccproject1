#pragma strict
private var green : Color = Color(0, 0.9, 0, 0.9);
private var yellow : Color = Color(0.9, 0.9,0,0.9);
var newMaterial : Material;
var target : Collider;
private var counter : int = 0;
var mySound : AudioClip;

function OnTriggerEnter(cubeTrigger : Collider)
{
if (cubeTrigger == target)
{

GetComponent.<Renderer>().material.color = green;
newMaterial.color = green;

GetComponent.<AudioSource>().PlayOneShot(mySound);
counter = counter + 1;
print("Collided: " + counter + " times!");
}
}



