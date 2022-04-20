using UnityEngine;

namespace FlockOfBirds
{
	public class BoidSystemSettings : SingletonMonoBehaviour<BoidSystemSettings>
	{
		[SerializeField]
		private Vector3 borderOrigin;

		[SerializeField]
		private float borderRadius;

		[SerializeField]
		private Transform target;

		public float BorderRadius
		{
			get
			{
				return borderRadius;
			}
		}

		public Vector3 BorderOrigin
		{
			get
			{
				return borderOrigin;
			}
		}

		public Vector3 TargetPosition
		{
			get
			{
				return target.position;
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawWireSphere(BorderOrigin, BorderRadius);
		}
	}
}