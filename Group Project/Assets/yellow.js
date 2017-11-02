#pragma strict
private var yellow : Color = Color(0.9, 0.9, 0, 0.9);
var newMaterial : Material;
var target : Collider;
private var counter : int = 0;
var mySound : AudioClip;

function OnTriggerEnter(cubeTrigger : Collider)
{
if (cubeTrigger == target)
{

GetComponent.<Renderer>().material.color = yellow;
newMaterial.color = yellow;

GetComponent.<AudioSource>().PlayOneShot(mySound);
counter = counter + 1;
print("Collided: " + counter + " times!");
}
}


