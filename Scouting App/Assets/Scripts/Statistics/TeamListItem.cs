using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TeamListItem : MonoBehaviour
{
	public Text TeamName;
	public Text TeamNum;
	public Image TeamStatus;

	public void ViewTeam()
	{
		PlayerPrefs.SetInt("currentTeam", int.Parse(TeamNum.text));
		SceneManager.LoadScene("view_team");
	}
}
