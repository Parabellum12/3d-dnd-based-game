
    [System.Serializable]
    public class AssetIDHandler
    {
        public int id;
        public string IDFileName;

        public AssetIDHandler(string _IDFileName, int _id)
        {
            IDFileName = _IDFileName;
            id = _id;
        }
    }
