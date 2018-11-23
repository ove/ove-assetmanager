namespace OVE.Service.Archives.Domain {

    public enum ProcessingStates {
        Error = -1,
        Unprocessed = 0,
        Processing = 1,
        CreatingDZI = 2,
        Uploading = 3,
        Processed = 4
    }

}