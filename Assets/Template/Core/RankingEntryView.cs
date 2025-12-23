using TMPro;
using UnityEngine;

namespace Template.Core
{
	public class RankingEntryView : MonoBehaviour
	{
		[SerializeField] private TMP_Text rankText;
		[SerializeField] private TMP_Text scoreText;
		[SerializeField] private TMP_Text nameText;
		[SerializeField] private GameObject trophyIcon;

		private void Awake()
		{
			// Defensive UI config: never let long names break the layout.
			if (nameText != null)
			{
				nameText.enableAutoSizing = true;
				nameText.enableWordWrapping = false;
				nameText.overflowMode = TextOverflowModes.Ellipsis;
				nameText.richText = false;
			}
		}

		public void Bind(int rank, int score, string playerName)
		{
			if (rankText != null) rankText.text = rank.ToString();
			if (scoreText != null) scoreText.text = score.ToString();
			if (nameText != null) nameText.text = RankingManager.SanitizePlayerName(playerName);
			
			// Show trophy icon only for top 3 ranks
			if (trophyIcon != null)
			{
				trophyIcon.SetActive(rank <= 3);
			}
		}
	}
}
