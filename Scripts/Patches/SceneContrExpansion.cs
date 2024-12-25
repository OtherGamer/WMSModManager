namespace WMSModManager.Patches
{
    public static class SceneContrExpansion
    {
        public static void LoadModScreen(this SceneContr sceneContr)
        {
            if (sceneContr.LoadingScreen != null)
            {
                sceneContr.LoadingScreen.SetActive(value: true);
            }
        }
    }
}
