﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EntityState {
    Invalid = -1,

    Spawn,
    Death, //died somehow

    PlayerLaunchReady, //ready for new move
    PlayerMove, //moving, wait to stop
    PlayerLock, //locked down to do other things
        
    Victory,

    PlayerLaunch
}