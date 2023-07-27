using System;
using UnityEngine;

namespace RoR2
{
	public class ModelPanelParameters : MonoBehaviour
	{
		public Transform focusPointTransform;
		public Transform cameraPositionTransform;
		public Quaternion modelRotation = Quaternion.identity;
		public float minDistance = 1f;
		public float maxDistance = 10f;
	}
	
	public class PrefabReferenceAttribute : PropertyAttribute {
	}
}
