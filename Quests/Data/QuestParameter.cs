public enum QuestParameter
{
    //Follower Parameters(UseFollower)
    FollowersUsed = 0,//int
    FollowerCivilization = 1,//civilization
    FollowerUseTrigger = 2,//Follower power, god refresh, buy a play
    
    //God Parameters(UseFollower, UseGod)
    GodName = 3,//string
    GodTier = 4,//god tier
    GodCivilization = 5,//civilization
    GodPower = 6,//god power
    
    //Level Parameters(UseFollower, UseGod, ScorePoints)
    LevelID = 7,//string
    LevelType = 8,//Lte/campaign
    LevelCivilization = 9,//civilization
    LevelIsPractice = 10,//bool
    
    //Mod Parameters(UseFollower, UseGod, ScorePoints)
    ModLimitGameType = 12,//Only for limited game mod
    
    //Piece Parameters(UseFollower, UseGod)
    PieceID = 13,//string
    PieceLevel = 14,//int
    
    
    //Trigger Specific
    //Score point only
    ScoreAdded = 15,//int
    //DoCascadeMerge only
    CascadeMergeLevel = 99,//int
    
    //FinishLevel only
    LevelScore = 98,//int 
    HasQuit = 97,//bool
    StarsScored = 96,
    StarsEarned = 95,
}
public enum QuestProgressionParameter
{
    Single = 8888,
    FollowersUsed = 0,//int
    ScoreAdded = 15,//int
    LevelScore = 98,//int 
    StarsScored = 96,//int 
    StarsEarned = 95,//int 
}