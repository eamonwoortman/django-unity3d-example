using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GameData {
    public enum Gametypes {
        JeuDeBoule,
        Blockgame
    }
    public Gametypes Gametype;
    public int Turn = 0;
    public float Score = 0;
}
