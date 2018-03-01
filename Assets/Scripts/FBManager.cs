using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using UnityEngine.UI;

public class FBManager : MonoBehaviour 
{
	public GameObject LoggedInUI;
	public GameObject NotLoggedInUI;
    public GameObject Friend;
	//初始化facebook
	void Awake() 
	{
		if(!FB.IsInitialized)
		{
			FB.Init(InitCallBack);
		}
		ShowUI();
	}

	//初始化成功的回调函数
	void InitCallBack()
	{
		Debug.Log("FB has been initialized");		
	}

	//登录函数
	public void Login()
	{
		if(!FB.IsLoggedIn)
		{
			Debug.Log("try to login");
			FB.LogInWithReadPermissions(new List<string>{"user_friends"},LoginCallBack);
		}
	}

	//登录的回调函数
	void LoginCallBack(ILoginResult result)
	{
		if(result.Error==null)
		{
			Debug.Log("FB has logged in");
			ShowUI();
		}
		else
		{
			Debug.Log("FB has not logged in,error:"+result.Error);
		}
	}

	//显示UI，登录前与登录后的切换
    void ShowUI()
    {
        if (FB.IsLoggedIn)
        {
            LoggedInUI.SetActive(true);
            NotLoggedInUI.SetActive(false);
            //获取facebook头像
            FB.API("me/picture?width=120&height=120", HttpMethod.GET, PictureCallBack);
            //获取facebook用户名
            FB.API("me?fields=first_name", HttpMethod.GET,NameCallBack);

            FB.API("me/friends",HttpMethod.GET,FriendsCallBack);
        }
        else
        {
            LoggedInUI.SetActive(false);
            NotLoggedInUI.SetActive(true);
        }
    }

    //获取头像的回调
    //获取头像并将Texture2D绘制到LoggedInUI的图片下
    void PictureCallBack(IGraphResult result)
    {        
        Texture2D image = result.Texture;
        LoggedInUI.transform.Find("HeadImage").GetComponent<Image>().sprite = Sprite.Create(image,new Rect(0, 0, 120,120),new Vector2(0.5f,0.5f));
    }

    //获取用户名的回调
    void NameCallBack(IGraphResult result)
    {
        Debug.Log(result.RawResult);
        IDictionary<string, object> profile = result.ResultDictionary;
        LoggedInUI.transform.Find("Username").GetComponent<Text>().text = "Username:"+profile["first_name"];
    }

    //获取登陆过此应用的朋友列表
    void FriendsCallBack(IGraphResult result)
    {
        Debug.Log(result.RawResult);
        IDictionary<string, object> data = result.ResultDictionary;
        List<object> friends = (List<object>)data["data"];
        foreach (object obj in friends)
        {
            Dictionary<string, object> dict = (Dictionary<string, object>)obj;
            CreateFriend(dict["name"].ToString(),dict["id"].ToString());
        }
    }

    //创建朋友
    void CreateFriend(string name, string id)
    {
        GameObject myFriend = Instantiate(Friend);
        Transform parent = LoggedInUI.transform.Find("ListContainer").Find("FriendList");
        myFriend.transform.SetParent(parent);
        myFriend.GetComponentInChildren<Text>().text = name;
        FB.API(id + "/picture?width=120&height=120", HttpMethod.GET, delegate (IGraphResult result) {
            myFriend.GetComponentInChildren<Image>().sprite = Sprite.Create(result.Texture, new Rect(0, 0, 120, 120), new Vector2(0.5f, 0.5f));
        });
    }

    //登出
    public void LogOut()
    {
        FB.LogOut();
        ShowUI();
    }

    //分享
    public void Share()
    {
        FB.ShareLink(new System.Uri("http://rkk9456.com"), "This game is awesome!", "Just a demo to test facebook in unity.",new System.Uri("http://rkk9456.com/Images/avator.png"));
    }

    //邀请
    public void Invite()
    {
        FB.AppRequest(message:"This game is so interesting!",title:"Come to play this game with me");
    }
}
