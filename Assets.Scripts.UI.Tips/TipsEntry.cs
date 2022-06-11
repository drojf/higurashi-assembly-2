using Assets.Scripts.Core;
using Assets.Scripts.Core.AssetManagement;
using Assets.Scripts.Core.State;
using UnityEngine;

namespace Assets.Scripts.UI.Tips
{
	public class TipsEntry : MonoBehaviour
	{
		private TipsDataEntry tip;

		private TipsManager manager;

		private UISprite sprite;

		private UIButton button;

		private bool isHover;

		public void Init(TipsDataEntry t, TipsManager mg)
		{
			tip = t;
			manager = mg;
			if (button == null)
			{
				button = GetComponent<UIButton>();
				sprite = GetComponent<UISprite>();
			}
			string name = $"tips_{t.Id:D3}na_normal";
			if (sprite.atlas.GetSprite(name) != null && AssetManager.Instance.UseNewArt)
			{
				button.normalSprite = $"tips_{t.Id:D3}na_normal";
				button.hoverSprite = $"tips_{t.Id:D3}na_hover";
				button.pressedSprite = $"tips_{t.Id:D3}na_hover";
				button.disabledSprite = $"tips_{t.Id:D3}na_normal";
			}
			else
			{
				button.normalSprite = $"tips_{t.Id:D3}_normal";
				button.hoverSprite = $"tips_{t.Id:D3}_hover";
				button.pressedSprite = $"tips_{t.Id:D3}_hover";
				button.disabledSprite = $"tips_{t.Id:D3}_normal";
			}
		}

		private void OnClick()
		{
			if (UICamera.currentTouchID >= -1 && tip != null && manager.isActive && GameSystem.Instance.GameState == GameState.TipsScreen)
			{
				(GameSystem.Instance.GetStateObject() as StateViewTips)?.OpenTips(tip.Script);
			}
		}

		private void OnHover(bool hover)
		{
			isHover = hover;
			if (tip != null)
			{
				if (!isHover)
				{
					manager.ClearTitle();
				}
				else if (GameSystem.Instance.GameState == GameState.TipsScreen)
				{
					manager.ShowTitle(GameSystem.Instance.UseEnglishText ? tip.Title : tip.TitleJp);
				}
			}
		}

		public void Reset()
		{
			tip = null;
			button = GetComponent<UIButton>();
			sprite = GetComponent<UISprite>();
			button.normalSprite = "tipslocked";
			button.hoverSprite = "tipslocked";
			button.pressedSprite = "tipslocked";
			button.disabledSprite = "tipslocked";
		}
	}
}
