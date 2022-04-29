using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.system;

public enum DisplaySystemState
{
    tilePosing,
    meeplePosing,
    turnStart,
    StateTransition,
    noState,
    idleState,
    endOfGame,
    scoreChange,
    gameStart
};

public class DisplaySystem : MonoBehaviour
{
    public static LayerMask TableLayer { get; private set; }
    public static LayerMask BoardLayer { get; private set; }

    // * BACK ************************************************
    [SerializeField] private CarcasheimBack system_back;
    [SerializeField] private Table table;
    [SerializeField] private Banner banner;
    [SerializeField] private PlayerList player_list;
    [SerializeField] private ScoreBoard score_board;

    // * STATE ***********************************************

    private DisplaySystemState act_system_state = DisplaySystemState.noState;
    private DisplaySystemState prev_system_state = DisplaySystemState.noState;
    private Queue<DisplaySystemState> state_transition;


    // * BOARD ***********************************************

    [SerializeField] private PlateauRepre board;
    // Plane board_plane;
    public TuileRepre reference_tile;


    // * PLAYER GESTION ***************************************
    public event System.Action<PlayerRepre> OnPlayerDisconnected;

    // * TILE *************************************************
    [SerializeField] private TuileRepre tuile_model;
    private List<TuileRepre> tiles_hand;
    private Queue<TuileRepre> tiles_drawned;
    private Queue<bool> lifespan_tiles_drawned;

    public TuileRepre act_tile;

    // * MEEPLE ***********************************************

    [SerializeField] private MeepleRepre meeple_model;
    private List<MeepleRepre> meeples_hand;
    public MeepleRepre act_meeple;
    public Dictionary<int, int> meeple_distrib;

    // * PLAYERS **********************************************

    private Dictionary<int, PlayerRepre> players_mapping;
    private PlayerRepre my_player;
    public PlayerRepre act_player;

    [SerializeField] private List<Color> players_color;

    [SerializeField] private bool DEBUG = false;



    // Start is called before the first frame update
    void Awake()
    {
        TableLayer = LayerMask.NameToLayer("Table");
        BoardLayer = LayerMask.NameToLayer("Board");

        players_mapping = new Dictionary<int, PlayerRepre>();
        state_transition = new Queue<DisplaySystemState>();

        meeples_hand = new List<MeepleRepre>();
        meeple_distrib = new Dictionary<int, int>();
        tiles_hand = new List<TuileRepre>();
        tiles_drawned = new Queue<TuileRepre>();
        lifespan_tiles_drawned = new Queue<bool>();

        if (players_color.Count == 0)
        {
            for (int i = 0; i < 10; i++)
            {
                players_color.Add(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
            }
        }
        //! TEST
        //gameBegin();
    }


    public void execAction(DisplaySystemAction action)
    {
        if (act_player != my_player)
        {
            switch (action.action_type)
            {
                case DisplaySystemActionTypes.tileSetCoord:
                    DisplaySystemActionTileSetCoord action_tsc = (DisplaySystemActionTileSetCoord)action;
                    if (act_tile != null && act_tile.Id == action_tsc.tile_id)
                    {
                        board.setTileAt(action_tsc.new_pos, act_tile);
                        table.tilePositionChanged(act_tile);
                    }
                    break;
                case DisplaySystemActionTypes.tileSelection:
                    DisplaySystemActionTileSelection action_ts = (DisplaySystemActionTileSelection)action;
                    if (action_ts.index_in_hand < 0 || action_ts.index_in_hand >= tiles_hand.Count || tiles_hand[action_ts.index_in_hand].Id != action_ts.tile_id)
                    {
                        action_ts.index_in_hand = -1;
                        for (int i = 0; i < tiles_hand.Count; i++)
                        {
                            if (tiles_hand[action_ts.index_in_hand].Id == action_ts.index_in_hand) { action_ts.index_in_hand = i; break; }
                        }
                    }
                    if (action_ts.index_in_hand != -1)
                        setSelectedTile(action_ts.index_in_hand, true);
                    break;
                case DisplaySystemActionTypes.meepleSelection:
                    DisplaySystemActionMeepleSelection action_ms = (DisplaySystemActionMeepleSelection)action;
                    {
                        if (action_ms.index_in_hand < 0 || action_ms.index_in_hand >= tiles_hand.Count || meeples_hand[action_ms.index_in_hand].Id != action_ms.meeple_id)
                        {
                            action_ms.index_in_hand = -1;
                            for (int i = 0; i < meeples_hand.Count; i++)
                            {
                                if (meeples_hand[i].Id == action_ms.meeple_id) { action_ms.index_in_hand = i; break; }
                            }
                        }
                        if (action_ms.index_in_hand != -1)
                            setSelectedMeeple(action_ms.index_in_hand, true);
                    }
                    break;
                case DisplaySystemActionTypes.meepleSetCoord:
                    DisplaySystemActionMeepleSetCoord action_msc = (DisplaySystemActionMeepleSetCoord)action;
                    if (board.getTileAt(action_msc.tile_pos) == act_tile && act_meeple != null && act_meeple.Id == action_msc.meeple_id)
                    {
                        bool meeple_pos_null = act_meeple.ParentTile == null;
                        if (act_tile.setMeeplePos(act_meeple, action_msc.slot_pos))
                        {
                            if (meeple_pos_null != (act_meeple.ParentTile == null))
                                table.meeplePositionChanged(act_meeple);
                        }
                    }
                    break;
                case DisplaySystemActionTypes.StateSelection:
                    DisplaySystemActionStateSelection action_ss = (DisplaySystemActionStateSelection)action;
                    setNextState(action_ss.new_state);
                    break;
            }
        }
    }

    public void setNextState(DisplaySystemState next_state)
    {
        state_transition.Enqueue(next_state);
        DisplaySystemState old_state = act_system_state;
        act_system_state = DisplaySystemState.StateTransition;
        if (state_transition.Count == 1)
        {
            stateLeave(old_state, next_state);
        }
    }

    void stateLeave(DisplaySystemState old_state, DisplaySystemState new_state)
    {
        //Debug.Log("Leaving " + old_state + " to " + new_state);
        switch (new_state)
        {
            case DisplaySystemState.meeplePosing:
                table.setBaseState(TableState.MeepleState);
                break;
            case DisplaySystemState.tilePosing:
                table.setBaseState(TableState.TileState);
                if (old_state == DisplaySystemState.turnStart)
                    banner.timerTour.startTimer();
                break;
            case DisplaySystemState.idleState:
                if ((DEBUG || act_player.is_my_player) && (old_state == DisplaySystemState.tilePosing || old_state == DisplaySystemState.meeplePosing))
                {
                    if (act_meeple != null)
                        system_back.sendTile(new TurnPlayParam(act_tile.Id, act_tile.Pos, act_meeple.Id, act_meeple.SlotPos));
                    else
                        system_back.sendTile(new TurnPlayParam(act_tile.Id, act_tile.Pos, -1, -1));
                }
                break;
            case DisplaySystemState.turnStart:
                table.setBaseState(TableState.TileState);
                banner.timerTour.resetStop();
                break;
        }

        switch (old_state)
        {
            case DisplaySystemState.meeplePosing:
                act_tile.hidePossibilities();
                break;

            case DisplaySystemState.idleState:
                TurnPlayParam play_param;
                system_back.getTile(out play_param);
                int index;
                // tuile posé
                if (play_param.id_tile != -1)
                {
                    if (act_tile == null || act_tile.Id != play_param.id_tile)
                    {
                        index = -1;
                        for (int i = 0; index < 0 && i < tiles_hand.Count; i++)
                        {
                            index = tiles_hand[i].Id == play_param.id_tile ? i : -1;
                        }
                        if (index == -1)
                        {
                            tiles_hand.Add(instantiateTileOfId(play_param.id_tile));
                            index = tiles_hand.Count - 1;
                        }
                        Debug.Log("Changing tile");
                        setSelectedTile(index, true);
                    }
                    board.finalizeTurn(play_param.tile_pos, act_tile);
                    table.tilePositionChanged(act_tile);

                    if (play_param.id_meeple != -1)
                    {
                        if (act_meeple == null || act_meeple.Id != play_param.id_meeple)
                        {
                            index = -1;
                            for (int i = 0; index == -1 && i < meeples_hand.Count; i++)
                            {
                                index = meeples_hand[i].Id == play_param.id_meeple ? i : -1;
                            }
                            if (index == -1)
                            {
                                //TODO creer meeple
                            }
                            setSelectedMeeple(index);
                        }
                        act_tile.setMeeplePos(act_meeple, play_param.slot_pos);
                        table.meeplePositionChanged(act_meeple);
                    }
                    else
                    {
                        if (act_meeple != null && act_meeple.ParentTile != null)
                        {
                            act_meeple.ParentTile = null;
                            table.meeplePositionChanged(act_meeple);
                        }
                    }
                }
                else
                {
                    if (act_tile != null && act_tile.Pos != null)
                    {
                        board.finalizeTurn(null, act_tile);
                        table.tilePositionChanged(act_tile);
                    }
                    if (act_meeple != null && act_meeple.ParentTile != null)
                    {
                        act_meeple.ParentTile = null;
                        table.meeplePositionChanged(act_meeple);
                    }
                }
                act_player = null;
                break;
            case DisplaySystemState.tilePosing:
                board.hideTilePossibilities();
                break;
        }
        prev_system_state = old_state;
    }


    void stateEnter(DisplaySystemState new_state, DisplaySystemState old_state)
    {
        //Debug.Log("State enterring from " + old_state + " to " + new_state);
        switch (new_state)
        {
            case DisplaySystemState.turnStart:
                act_player = players_mapping[system_back.getNextPlayer()];
                Debug.Log("Tour de " + act_player.Name + " " + act_player.Id.ToString());
                if (old_state != DisplaySystemState.gameStart)
                    player_list.nextPlayer(act_player);
                if (my_player == null && act_player.is_my_player)
                    banner.setPlayer(act_player);
                table.Focus = act_player.is_my_player;
                turnBegin();
                break;
            case DisplaySystemState.tilePosing:
                if (old_state == DisplaySystemState.turnStart)
                {
                    if (tiles_hand.Count > 0)
                        setSelectedTile(0, true);
                    if (meeples_hand.Count > 0)
                        setSelectedMeeple(0, true);
                }
                board.displayTilePossibilities();
                break;
            case DisplaySystemState.meeplePosing:
                foreach (MeepleRepre mp in meeples_hand)
                {
                    mp.slot_possible.Clear();
                    system_back.askMeeplePosition(new MeeplePosParam(act_tile.Id, act_tile.Pos, mp.Id), mp.slot_possible);
                }
                if (act_meeple != null)
                    act_tile.showPossibilities(act_player, act_meeple.slot_possible);
                break;
            case DisplaySystemState.scoreChange:
                List<PlayerScoreParam> scores = new List<PlayerScoreParam>();
                List<Zone> zones = new List<Zone>();
                system_back.askScores(scores, zones);
                foreach (PlayerScoreParam score in scores)
                {
                    players_mapping[(int)score.id_player].Score = score.points_gagnes;
                    Debug.Log("score for " + score.id_player + " " + score.points_gagnes);
                    foreach (Zone z in zones)
                    {
                        foreach (System.Tuple<int, int, ulong> pos in z.positions)
                        {
                            TuileRepre tl = board.getTileAt(new PositionRepre(pos.Item1, pos.Item2));
                            if (tl != null)
                            {
                                SlotIndic slt = tl.getSlotAt((int)pos.Item3);
                                if (slt != null)
                                {
                                    slt.clean();
                                }
                            }
                        }
                    }
                }
                break;
            case DisplaySystemState.gameStart:
                gameBegin();
                break;
            case DisplaySystemState.endOfGame:
                table.Focus = false;
                List<PlayerScoreParam> scores_final = new List<PlayerScoreParam>();
                List<Zone> zones_final = new List<Zone>();
                system_back.askFinalScore(scores_final, zones_final);

                if (my_player != null)
                {
                    foreach (PlayerScoreParam score in scores_final)
                    {
                        players_mapping[(int)score.id_player].Score = score.points_gagnes;
                        Debug.Log("score for " + score.id_player + " " + score.points_gagnes);
                    }

                    int n_sup = 0;
                    foreach (PlayerRepre player in players_mapping.Values)
                    {
                        if (player.Id != my_player.Id && my_player.Score < player.Score)
                        {
                            n_sup += 1;
                        }

                    }
                    score_board.setEndOfGame(my_player, 1 + n_sup);
                }
                else
                {
                    PlayerRepre max_player = null;

                    foreach (PlayerRepre pl in players_mapping.Values)
                    {
                        if (max_player == null || max_player.Score < pl.Score)
                            max_player = pl;
                    }

                    score_board.setEndOfGame(max_player, 1);
                }

                break;
        }
        act_system_state = new_state;
    }

    void tableCheck(Ray ray, ref bool consumed)
    {
        RaycastHit hit;
        if ((true || act_player.is_my_player) && Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << TableLayer)))
        {
            consumed = table.colliderHit(hit.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool mouse_consumed = (act_system_state != DisplaySystemState.tilePosing && act_system_state != DisplaySystemState.meeplePosing);
        if (!mouse_consumed && Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            tableCheck(ray, ref mouse_consumed);

            RaycastHit hit = new RaycastHit();
            bool hit_valid = (DEBUG || act_player.is_my_player) && !mouse_consumed && Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << BoardLayer));
            /*
                        float enter;
                        if (!hit_valid && !mouse_consumed && board_plane.Raycast(ray, out enter))
                        {
                            if (act_system_state == DisplaySystemState.tilePosing && hit.transform.tag == "TileIndicCollider" && act_tile != null)
                            {
                                Vector3 p = ray.GetPoint(enter);
                                if (act_system_state == DisplaySystemState.tilePosing && act_tile != null)
                                {
                                    board.setTileAt(act_tile.possibilitiesPosition[Random.Range(0, act_tile.possibilitiesPosition.Count)], act_tile);
                                    act_tile.transform.position = p;
                                    table.tilePositionChanged(act_tile);
                                }
                            }
                            else
                                Debug.Log("PROBLEM " + hit.transform.tag);
                        }
            */
            if (hit_valid)
            {
                SlotIndic slot;
                switch (hit.transform.tag)
                {
                    case "TileIndicCollider":
                        if (act_system_state == DisplaySystemState.tilePosing && act_tile != null)
                        {
                            TileIndicator tile_indic = hit.transform.parent.GetComponent<TileIndicator>();
                            if (board.setTileAt(tile_indic.position, act_tile))
                            {
                                table.tilePositionChanged(act_tile);
                            }
                            else
                                Debug.Log("Tuile non placée à " + tile_indic.position.ToString());
                            system_back.sendAction(new DisplaySystemActionTileSetCoord(act_tile.Id, act_tile.Pos));
                        }
                        break;

                    case "SlotCollider":
                        if (act_system_state == DisplaySystemState.meeplePosing && act_meeple != null)
                        {
                            slot = hit.transform.parent.GetComponent<SlotIndic>();
                            if (hit.transform.parent.parent != act_tile.pivotPoint)
                                Debug.Log("Wrong parent " + hit.transform.parent.parent.name + " instead of " + act_tile.pivotPoint.name);
                            else
                            {
                                bool meeple_position_is_null = act_meeple.ParentTile == null;
                                if (act_tile.setMeeplePos(act_meeple, slot))
                                {
                                    if (meeple_position_is_null != (act_meeple.ParentTile == null))
                                        table.meeplePositionChanged(act_meeple);
                                }
                                system_back.sendAction(new DisplaySystemActionMeepleSetCoord(act_tile.Id, act_tile.Pos, act_meeple.Id, act_meeple.SlotPos));
                            }
                        }
                        break;
                    case "TileBodyCollider":
                        if (act_system_state == DisplaySystemState.tilePosing && act_tile != null)
                        {
                            PositionRepre rotation;
                            if (hit.transform.parent != act_tile.pivotPoint)
                                Debug.Log("Tried to rotate " + hit.transform.parent.name + " instead of act tile");
                            else if (act_tile.nextRotation(out rotation))
                            {
                                board.setTileAt(rotation, act_tile);
                                system_back.sendAction(new DisplaySystemActionTileSetCoord(act_tile.Id, act_tile.Pos));
                            }
                        }
                        break;
                    default:
                        Debug.Log("Hit " + hit.transform.tag + " collider in system");
                        break;
                }
            }
        }

        switch (act_system_state)
        {
            case DisplaySystemState.meeplePosing:
                if (act_player.is_my_player || DEBUG)
                {
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        setNextState(DisplaySystemState.idleState);
                        system_back.sendAction(new DisplaySystemActionStateSelection(DisplaySystemState.idleState));
                    }
                    else if (Input.GetKeyDown(KeyCode.Backspace))
                    {
                        setNextState(DisplaySystemState.tilePosing);
                        system_back.sendAction(new DisplaySystemActionStateSelection(DisplaySystemState.tilePosing));
                    }
                }
                break;
            case DisplaySystemState.tilePosing:
                if ((act_player.is_my_player || DEBUG) &&
                    Input.GetKeyDown(KeyCode.Return) &&
                    act_tile != null &&
                    act_tile.Pos != null)
                {
                    if (meeples_hand.Count > 0)
                    {
                        setNextState(DisplaySystemState.meeplePosing);
                        system_back.sendAction(new DisplaySystemActionStateSelection(DisplaySystemState.meeplePosing));
                    }
                    else
                    {
                        setNextState(DisplaySystemState.idleState);
                        system_back.sendAction(new DisplaySystemActionStateSelection(DisplaySystemState.idleState));
                    }
                }
                break;
            case DisplaySystemState.turnStart:
                break;
            case DisplaySystemState.StateTransition:
                act_system_state = state_transition.Dequeue();
                stateEnter(act_system_state, prev_system_state);
                if (state_transition.Count > 0)
                {
                    stateLeave(act_system_state, state_transition.Peek());
                    act_system_state = DisplaySystemState.StateTransition;
                }
                break;
        }
    }

    public void gameBegin()
    {
        Debug.Log("HERE");
        WinCondition win = WinCondition.WinByTime;
        List<int> param = new List<int>();
        system_back.askWinCondition(ref win, param);
        Debug.Log("HERE " + win + " " + param);

        List<PlayerInitParam> players_init = new List<PlayerInitParam>();
        system_back.askPlayersInit(players_init);
        Debug.Log("HERE");
        my_player = null;
        int my_player_id = system_back.getMyPlayer();


        int L = players_init.Count;
        for (int i = 0; i < L; i++)
        {
            PlayerRepre pl = new PlayerRepre(players_init[i], players_color[i]);
            pl.is_my_player = pl.Id == my_player_id || my_player_id == -1;
            if (pl.is_my_player && my_player_id != -1)
                my_player = pl;
            players_mapping.Add(pl.Id, pl);
            player_list.addPlayer(pl);
        }

        int first_tile = system_back.askIdTileInitial();
        Debug.Log("HERE");
        TuileRepre tl = instantiateTileOfId(first_tile);
        board.setTileAt(new PositionRepre(0, 0, 0), tl);

        int min, sec;
        system_back.askTimerTour(out min, out sec);
        banner.setTimerTour(min, sec);
        banner.setPlayerNumber(L);
        if (my_player != null)
            banner.setPlayer(my_player);

        banner.setWinCondition(win, table, param);
        setNextState(DisplaySystemState.turnStart);
    }

    public TuileRepre instantiateTileOfId(int id)
    {
        TuileRepre model;
        model = Resources.Load<TuileRepre>("tile" + id.ToString());
        if (model == null)
        {
            Debug.Log("Tried to log unknown tile " + id.ToString());
            model = tuile_model;
        }
        TuileRepre tl = Instantiate<TuileRepre>(model);
        tl.Id = id;
        return tl;
    }

    public void turnBegin()
    {
        List<MeepleInitParam> meeples_init = new List<MeepleInitParam>();
        List<TileInitParam> tiles_init = new List<TileInitParam>();

        act_meeple = null;
        act_tile = null;

        tiles_hand.Clear();

        meeples_hand.Clear();
        meeple_distrib.Clear();

        int final_count = system_back.askTilesInit(tiles_init);
        int L = tiles_init.Count;
        TuileRepre tl;
        for (int i = 0; i < L; i++)
        {
            tl = instantiateTileOfId(tiles_init[i].id_tile);
            if (tiles_init[i].tile_flags)
                system_back.getTilePossibilities(tl.Id, tl.possibilitiesPosition);
            tiles_drawned.Enqueue(tl);
            lifespan_tiles_drawned.Enqueue(tiles_init[i].tile_flags);
        }

        system_back.askMeeplesInit(meeples_init);
        L = meeples_init.Count;
        for (int i = 0; i < L; i++)
        {
            // TODO should instantiate dependnat on the type
            MeepleRepre mp = Instantiate<MeepleRepre>(meeple_model);
            mp.color.material.color = act_player.color;
            mp.Id = meeples_init[i].id_meeple;
            meeples_hand.Add(mp);
            meeple_distrib[mp.Id] = meeples_init[i].nb_meeple;

            mp.slot_possible.Clear();

        }


        table.resetHandSize(final_count, meeples_hand, meeple_distrib);
    }

    public void askPlayerOrder(LinkedList<PlayerRepre> players)
    {
        List<int> players_id = new List<int>();
        players.Clear();
        system_back.askPlayerOrder(players_id);
        for (int i = 0; i < players_id.Count; i++)
            players.AddLast(players_mapping[players_id[i]]);
    }

    public TuileRepre getNextTile(out bool perma)
    {
        perma = false;
        if (tiles_drawned.Count == 0)
        {
            setNextState(DisplaySystemState.tilePosing);
            return null;
        }

        TuileRepre tile = tiles_drawned.Dequeue();

        perma = lifespan_tiles_drawned.Dequeue();
        if (perma)
        {
            tiles_hand.Add(tile);
        }
        return tile;
    }

    public void setSelectedTile(int index, bool forced = false)
    {
        if (0 <= index && index < tiles_hand.Count && (act_system_state == DisplaySystemState.tilePosing || forced))
        {

            TuileRepre n_tuile = tiles_hand[index];
            if (n_tuile == act_tile)
                return;
            if (act_tile != null && act_tile.Pos != null)
            {
                board.setTileAt(null, act_tile);
                table.tilePositionChanged(act_tile);
            }
            if (act_meeple != null && act_meeple.ParentTile != null)
            {
                act_meeple.ParentTile = null;
                table.meeplePositionChanged(act_meeple);
            }
            table.activeTileChanged(act_tile, n_tuile);
            act_tile = n_tuile;
            if (act_system_state == DisplaySystemState.tilePosing)
                board.setTilePossibilities(act_player, act_tile);
            if (act_player.is_my_player)
                system_back.sendAction(new DisplaySystemActionTileSelection(act_tile.Id, index));
        }
        else
        {
            Debug.Log("Invalid tile access " + index.ToString() + " " + act_system_state.ToString());
        }
    }

    public void setSelectedMeeple(int index, bool forced = false)
    {
        // Debug.Log("Meeple posing : " + index.ToString() + " " + meeples_hand.Count);
        if (0 <= index && index < meeples_hand.Count && (act_system_state == DisplaySystemState.meeplePosing || forced))
        {
            MeepleRepre n_meeple = meeples_hand[index];
            if (n_meeple == act_meeple)
                return;
            if (act_meeple != null && act_meeple.ParentTile != null)
            {
                act_meeple.ParentTile = null;
                table.meeplePositionChanged(act_meeple);
            }
            table.activeMeepleChanged(act_meeple, n_meeple);
            act_meeple = n_meeple;
            if (act_system_state == DisplaySystemState.meeplePosing && act_meeple != null)
            {
                act_tile.showPossibilities(act_player, act_meeple.slot_possible);
            }
            if (act_player.is_my_player)
                system_back.sendAction(new DisplaySystemActionMeepleSelection(act_meeple.Id, index));
        }
        else
        {
            Debug.Log("Invalid meeple access " + index.ToString() + " " + act_system_state.ToString());
        }
    }
}
