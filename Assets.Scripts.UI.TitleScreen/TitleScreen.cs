using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Assets.Scripts.UI.TitleScreen
{
	public class TitleScreen : MonoBehaviour
	{
		public TweenAlpha BackgroundTween;

		public UITexture BackgroundTexture;

		public Texture2D BG1;

		public Texture2D BG2;

		public List<UISprite> Sprites;

		public bool IsActive = true;

		private IEnumerator LeaveMenuAnimation(MenuUIController.MenuCloseDelegate onClose)
		{
			BackgroundTween.PlayReverse();
			yield return new WaitForSeconds(0.5f);
			onClose?.Invoke();
			Object.Destroy(base.gameObject);
		}

		public void FadeOut()
		{
			BackgroundTween.PlayReverse();
		}

		public void FadeIn()
		{
			BackgroundTween.PlayForward();
		}

		public void Leave(MenuUIController.MenuCloseDelegate onClose)
		{
			for (int i = 0; i < Sprites.Count; i++)
			{
				TitleScreenButton component = Sprites[i].GetComponent<TitleScreenButton>();
				if (component != null)
				{
					component.IsLeaving = true;
				}
			}
			StartCoroutine(LeaveMenuAnimation(onClose));
		}

		private void OpeningAnimation()
		{
			for (int j = 0; j < Sprites.Count; j++)
			{
				int i = j;
				LeanTween.value(Sprites[j].gameObject, delegate(float f)
				{
					Sprites[i].color = new Color(1f, 1f, 1f, f);
				}, 0f, 1f, 1f).delay = (float)j * 0.4f;
			}
		}

		public void Enter()
		{
			foreach (UISprite sprite in Sprites)
			{
				sprite.color = new Color(1f, 1f, 1f, 0f);
			}
			OpeningAnimation();
		}
	}
}
