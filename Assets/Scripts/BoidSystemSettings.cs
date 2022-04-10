using UnityEngine;

namespace FlockOfBirds
{
	public class BoidSystemSettings : SingletonMonoBehaviour<BoidSystemSettings>
	{
		[SerializeField]
		private Vector3 borderOrigin;

		[SerializeField]
		private float borderRadius;

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

		private void OnDrawGizmos()
		{
			Gizmos.DrawWireSphere(BorderOrigin, BorderRadius);
		}
	}
}