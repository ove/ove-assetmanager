const Constants = {
    /**************************************************************
                                Viewer
    **************************************************************/
    BUFFER_STATUS_BROADCAST_FREQUENCY: 700,
    RESCALE_DURING_REFRESH_TIMEOUT: 1000,

    /**************************************************************
                              Controller
    **************************************************************/
    DEFAULT_STATE_NAME: 'DSIIntro',

    /**************************************************************
                             Video Player
    **************************************************************/
    VIDEO_READY_TIMEOUT: 500,
    STARTING_TIME: 0,
    STANDARD_RATE: 1,
    YOUTUBE_PLAYER_LOADED_TEST_INTERVAL: 1000,
    YOUTUBE_PLAYBACK_LOOP_TEST_INTERVAL: 100,

    /**************************************************************
                               Back-end
    **************************************************************/
    SOCKET_READY_WAIT_TIME: 3000,
    OPERATION_SYNC_DELAY: 350,
    HTTP_HEADER_CONTENT_TYPE: 'Content-Type',
    HTTP_CONTENT_TYPE_JSON: 'application/json',

    /**************************************************************
                                Common
    **************************************************************/
    MIN_BUFFERED_PERCENTAGE: 15,
    CONTENT_DIV: '#video_player',
    APP_NAME: 'videos'
};

/**************************************************************
                            Enums
**************************************************************/
Constants.Operation = {
    PLAY: 'play',
    PAUSE: 'pause',
    STOP: 'stop',
    SEEK: 'seekTo',
    BUFFER_STATUS: 'bufferStatus'
};

Constants.BufferStatus = {
    COMPLETE: 'complete',
    BUFFERING: 'buffering'
};

exports.Constants = Constants;