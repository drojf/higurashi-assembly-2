using Assets.Scripts.Core;
using Assets.Scripts.Core.State;
using UnityEngine;

namespace Assets.Scripts.UI.Tips
{
	public delegate void ClickHandler();

	public class TipsButton : MonoBehaviour
	{
		private TipsManager manager;

		// We steal this button for use in other views because making buttons from scratch is a pain
		// In those cases, this click handler will be used instead of `manager`
		private ClickHandler clickHandler;

		private bool isEnabled = true;

		public void Setup(TipsManager mg)
		{
			manager = mg;
		}

		public void Setup(ClickHandler clickHandler)
		{
			this.clickHandler = clickHandler;
		}

		public void Disable()
		{
			isEnabled = false;
		}

		private void OnClick()
		{
			if (clickHandler != null)
			{
				clickHandler();
				return;
			}
			GameSystem instance = GameSystem.Instance;
			if (instance.GameState == GameState.TipsScreen && isEnabled && manager.isActive && UICamera.currentTouchID == -1)
			{
				StateViewTips stateViewTips = instance.GetStateObject() as StateViewTips;
				if (stateViewTips != null)
				{
					switch (base.name)
					{
					case "PageLeft":
						manager.ChangePage(-1);
						break;
					case "PageRight":
						manager.ChangePage(1);
						break;
					case "ExitButton":
						stateViewTips.RequestLeave();
						instance.CanSave = true;
						break;
					}
				}
			}
		}
	}
}
