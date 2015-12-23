using UnityEngine;
using System.Collections;
using Facebook.Unity;
using System.Collections.Generic;
using UnityEngine.UI;

public class FBManager : MonoBehaviour {

	public GameObject UIFBIsLoggedIn;
	public GameObject UIFBNotLoggedIn;
	public GameObject UIFBAvatar;
	public GameObject UIFBUserName;

	public Text ScoresDebug;
	private List<object> scoresList = null;

	public GameObject ScoreEntryPanel;
	public GameObject ScoreScrollList;

	private Dictionary<string,string> profile = null;

	public Button playButton;
	public Button shopButton;
	public Button settingsButton;
	public Button creditsButton;
	public Button shareButton;
	public Button leaderboardButton;
	public Button achievementsButton;

	void Awake(){
		FB.Init (SetInit, OnHideUnity);
	}

	void Start(){
		playButton = playButton.GetComponent<Button> ();
		shopButton = shopButton.GetComponent<Button> ();
		shareButton = shareButton.GetComponent<Button> ();
		creditsButton = creditsButton.GetComponent<Button> ();
		achievementsButton = achievementsButton.GetComponent<Button> ();
		leaderboardButton = leaderboardButton.GetComponent<Button> ();
		settingsButton=settingsButton.GetComponent<Button>();
	}

	private void SetInit(){
		Debug.Log ("FB Init done");

		if (FB.IsLoggedIn) {
			Debug.Log("FB logged int");
			DealWithFBMenus(true);
		} else {
			DealWithFBMenus(false);
		}
	}

	private void OnHideUnity(bool isGameShown){
		if (!isGameShown) {
			Time.timeScale = 0;

		} else {
			Time.timeScale = 1;

		}
	}

	public void FBLogin(){
		FB.LogInWithReadPermissions (new List<string>(){"email,publish_actions"},AuthCallback);
		playButton.interactable = false;
		shopButton.interactable = false;
		shareButton.interactable = false;
		creditsButton.interactable = false;
		achievementsButton.interactable=false;
		leaderboardButton.interactable=false;
		settingsButton.interactable=false;
	}
	
	void AuthCallback(IResult result){
		if (FB.IsLoggedIn) {
			Debug.Log ("FB login worked");
			DealWithFBMenus(true);
			playButton.interactable = true;
			shopButton.interactable = true;
			shareButton.interactable = true;
			creditsButton.interactable = true;
			achievementsButton.interactable=true;
			leaderboardButton.interactable=true;
			settingsButton.interactable=true;
		} else {
			Debug.Log("FB login failed");
			DealWithFBMenus(false);
			playButton.interactable = true;
			shopButton.interactable = true;
			shareButton.interactable = true;
			creditsButton.interactable = true;
			achievementsButton.interactable=true;
			leaderboardButton.interactable=true;
			settingsButton.interactable=true;
		}
	}

	void DealWithFBMenus(bool isLoggedIn){
		if (isLoggedIn) {
			UIFBIsLoggedIn.SetActive(true);
			UIFBNotLoggedIn.SetActive(false);

			//get profile picture
			FB.API (Util.GetPictureURL("me",128,128),Facebook.Unity.HttpMethod.GET,DealWithProfilePicture);

			//get username
			FB.API("/me?fields=id,first_name", Facebook.Unity.HttpMethod.GET, DealWithUserName);

		} else {
			UIFBIsLoggedIn.SetActive(false);
			UIFBNotLoggedIn.SetActive(true);
		}
	}

	void DealWithProfilePicture(IGraphResult result){
		if (result.Error != null) {
			Debug.Log("Problem with getting profile picture");
			return;
		} else {
			Image userAvatar = UIFBAvatar.GetComponent<Image>();
			userAvatar.sprite = Sprite.Create(result.Texture,new Rect(0,0,128,128),new Vector2(0,0));
		}
	}

	void DealWithUserName(IGraphResult result){
		if (result.Error != null) {
			Debug.Log("Problem with getting username");
			return;
		} else {
			profile = Util.DeserializeJSONProfile(result.RawResult);
			Text userMsg = UIFBUserName.GetComponent<Text>();
			userMsg.text = "Hi, " + profile["first_name"];
		}
	}

	public void ShareWithFriends(){
		playButton.interactable = false;
		shopButton.interactable = false;
		shareButton.interactable = false;
		creditsButton.interactable = false;
		achievementsButton.interactable=false;
		leaderboardButton.interactable=false;
		settingsButton.interactable=false;
		FB.FeedShare (
			string.Empty, //toId
			new System.Uri("https://www.facebook.com/groups/838406229589072/"), //link
			"Play this game!!!", //linkName
			"Best game ever!!!", //linkCaption
			"LinkDescription", //linkDescription
			new System.Uri("https://www.facebook.com/photo.php?fbid=10206747766326763&set=gm.879371872159174&type=3&theater"), //picture
			string.Empty, //mediaSource
			LogCallback //callback
			);
	}

	void LogCallback(IResult result){
		if (result.Error != null) {
			Debug.Log ("Share didn't work");
			StartCoroutine(SetButtonsInteractable());

		} else {
			Debug.Log("Share worked");
			StartCoroutine(SetButtonsInteractable());
		}

	}

	private IEnumerator SetButtonsInteractable(){
		yield return new WaitForSeconds (1.0f);
		playButton.interactable = true;
		shopButton.interactable = true;
		shareButton.interactable = true;
		creditsButton.interactable = true;
		achievementsButton.interactable=true;
		leaderboardButton.interactable=true;
		settingsButton.interactable=true;
	}

	public void InviteFriends(){

		playButton.interactable = false;
		shopButton.interactable = false;
		shareButton.interactable = false;
		creditsButton.interactable = false;
		achievementsButton.interactable=false;
		leaderboardButton.interactable=false;
		settingsButton.interactable=false;

		FB.AppRequest (
			"This game is awesome!!!Join me",
			null,null,null,null,null,
			"Invite your friends to join you",
			InviteCallback
			);
	}

	public void InviteCallback(IResult result){
		if (result.Error != null) {
			Debug.Log ("Invite didn't work");
			StartCoroutine(SetButtonsInteractable());
			
		} else {
			Debug.Log("Invite worked");
			StartCoroutine(SetButtonsInteractable());
		}

	}

	//Scores API

	public void QueryScores(){
		FB.API ("/app/scores?fields=score,user.limit(30)",Facebook.Unity.HttpMethod.GET,ScoresCallback);
	}

	private void ScoresCallback(IResult result){
		Debug.Log ("Scores callback:" + result.RawResult);
		ScoresDebug.text = "";
		scoresList = Util.DeserializeScores (result.RawResult);

		foreach(Transform child in ScoreScrollList.transform){

			GameObject.Destroy(child.gameObject);
		}

		foreach (object score in scoresList) {
			var entry = (Dictionary<string,object>) score;
			var user = (Dictionary<string,object>) entry["user"];

			ScoresDebug.text = ScoresDebug.text + "UN: " + user["name"] + "-" + entry["score"] + ", ";
		
			GameObject ScorePanel = Instantiate(ScoreEntryPanel) as GameObject;
			ScorePanel.transform.SetParent(ScoreScrollList.transform,false);

			Transform ThisScoreName = ScorePanel.transform.Find("FriendName");
			Transform ThisScoreScore = ScorePanel.transform.Find("FriendScore");
			Text ScoreName = ThisScoreName.GetComponent<Text>();
			Text ScoreScore = ThisScoreScore.GetComponent<Text>();

			ScoreName.text = user["name"].ToString();
			ScoreScore.text = entry["score"].ToString ();

			Transform ThisUserAvatar = ScorePanel.transform.Find("FriendAvatar");
			Image UserAvatar = ThisUserAvatar.GetComponent<Image>();

			FB.API (
				Util.GetPictureURL(user["id"].ToString(),128,128),Facebook.Unity.HttpMethod.GET,delegate(IGraphResult pictureResult) {
				if(result.Error != null){ //if there was an error
					Debug.Log("Couldn't find profile picture"); //remember to put a default picture

				}else{ //if everything was fine

					UserAvatar.sprite = Sprite.Create(pictureResult.Texture,new Rect(0,0,128,128),new Vector2(0,0));
				}
			}

				);
		}
	}

	public void SetScore(){

		var scoreData = new Dictionary<string,string> () {{"score", Random.Range(10,200).ToString()}};

		FB.API ("/me/scores",Facebook.Unity.HttpMethod.POST,delegate (IGraphResult result){
			Debug.Log("Score submit result: " + result.RawResult);
	},scoreData);
	}
}
