using System.IO;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

using Codice.Client.BaseCommands;
using Codice.Client.Common;
using Unity.PlasticSCM.Editor.AssetUtils.Processor;

namespace Unity.PlasticSCM.Editor.AssetUtils
{
    internal static class SaveAssets
    {
        internal static void ForChangesWithConfirmation(
            List<ChangeInfo> changes,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            out bool isCancelled)
        {
            ForPaths(
                GetPaths(changes), true,
                workspaceOperationsMonitor,
                out isCancelled);
        }

        internal static void ForPathsWithConfirmation(
            List<string> paths,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            out bool isCancelled)
        {
            ForPaths(
                paths, true,
                workspaceOperationsMonitor,
                out isCancelled);
        }

        internal static void ForChangesWithoutConfirmation(
            List<ChangeInfo> changes,
            WorkspaceOperationsMonitor workspaceOperationsMonitor)
        {
            bool isCancelled;
            ForPaths(
                GetPaths(changes), false,
                workspaceOperationsMonitor,
                out isCancelled);
        }

        internal static void ForPathsWithoutConfirmation(
            List<string> paths,
            WorkspaceOperationsMonitor workspaceOperationsMonitor)
        {
            bool isCancelled;
            ForPaths(
                paths, false,
                workspaceOperationsMonitor,
                out isCancelled);
        }

        static void ForPaths(
            List<string> paths,
            bool askForUserConfirmation,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            out bool isCancelled)
        {
            workspaceOperationsMonitor.Disable();
            try
            {
                SaveDirtyScenes(
                    paths,
                    askForUserConfirmation,
                    out isCancelled);

                if (isCancelled)
                    return;

                AssetDatabase.SaveAssets();
            }
            finally
            {
                workspaceOperationsMonitor.Enable();
            }
        }

        static void SaveDirtyScenes(
            List<string> paths,
            bool askForUserConfirmation,
            out bool isCancelled)
        {
            isCancelled = false;

            List<Scene> scenesToSave = new List<Scene>();

            foreach (Scene dirtyScene in GetDirtyScenes())
            {
                if (Contains(paths, dirtyScene))
                    scenesToSave.Add(dirtyScene);
            }

            if (scenesToSave.Count == 0)
                return;

            if (askForUserConfirmation)
            {
                isCancelled = !EditorSceneManager.
                    SaveModifiedScenesIfUserWantsTo(
                        scenesToSave.ToArray());
                return;
            }

            EditorSceneManager.SaveScenes(
                scenesToSave.ToArray());
        }

        static List<Scene> GetDirtyScenes()
        {
            List<Scene> dirtyScenes = new List<Scene>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (!scene.isDirty)
                    continue;

                dirtyScenes.Add(scene);
            }

            return dirtyScenes;
        }

        static bool Contains(
            List<string> paths,
            Scene scene)
        {
            if (string.IsNullOrEmpty(scene.path))
                return false;

            foreach (string path in paths)
            {
                if (PathHelper.IsSamePath(
                        path,
                        Path.GetFullPath(scene.path)))
                    return true;
            }

            return false;
        }

        static List<string> GetPaths(
            List<ChangeInfo> changeInfos)
        {
            List<string> result = new List<string>();
            foreach (ChangeInfo change in changeInfos)
                result.Add(change.GetFullPath());
            return result;
        }
    }
}
