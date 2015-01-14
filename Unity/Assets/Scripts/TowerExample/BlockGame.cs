using UnityEngine;
using System.Collections;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class BlockGame : BaseGame<JeuDeBoulesData> {
    /*private LoginMenu loginMenu;
    private SavegameMenu savegameMenu;*/
    /*
	void Start () {
        loginMenu = GetComponent<LoginMenu>();
        loginMenu.HasLoggedIn += LoggedIn;

        savegameMenu = GetComponent<SavegameMenu>();
        savegameMenu.enabled = false;
        savegameMenu.OnLoadButtonPressed += OnLoadButtonPressed;
        savegameMenu.OnSaveButtonPressed += OnSaveButtonPressed;

        Data = new GameData() { Gametype = GameData.Gametypes.Blockgame };
	}

    private void OnLoadButtonPressed(string filename) {

    }
    private void OnSaveButtonPressed(string filename) {

    }
    private void LoggedIn() {
        savegameMenu.LoadSavegames();
        Invoke("DisableLoginMenu", 1);
    }

    private void DisableLoginMenu() {
        loginMenu.enabled = false;
        savegameMenu.enabled = true;
    }
    */
    public override void Deserialize(JeuDeBoulesData data) {
        Debug.Log("Parse stuff");
    }

    protected override JeuDeBoulesData Serialize() {
        Block[] block = FindObjectsOfType<Block>();
        // Create an array containing the BallData from all balls in the scene
        BlockData[] ballData = block.Select(ball => ball.Data).ToArray();

        JObject jsonObject = new JObject();
        jsonObject.Add("game", JsonConvert.SerializeObject(Data));
        jsonObject.Add("blocks", JsonConvert.SerializeObject(ballData));

        return Data;
    }
}
