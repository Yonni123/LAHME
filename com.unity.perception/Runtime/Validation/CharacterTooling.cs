using System.Collections.Generic;
using static UnityEngine.Perception.Content.CharacterValidation;
using System.Linq;

namespace UnityEngine.Perception.Content
{
    public class CharacterTooling : MonoBehaviour
    {
        public bool CharacterRequiredBones(Animator animator, out Dictionary<HumanBone, bool> failed)
        {
            var result = AvatarRequiredBones(animator);
            failed = new Dictionary<HumanBone, bool>();

            for (int i = 0; i < result.Count; i++)
            {
                var bone = result.ElementAt(i);
                var boneKey = bone.Key;
                var boneValue = bone.Value;

                if (boneValue != true)
                    failed.Add(boneKey, boneValue);
            }

            if (failed.Count == 0)
                return true;

            return false;
        }

        public bool CharacterPoseData(GameObject gameObject, out List<GameObject> failedGameObjects)
        {
            failedGameObjects = new List<GameObject>();

            var componentsParent = gameObject.GetComponents<Transform>();
            var componentsChild = gameObject.GetComponentsInChildren<Transform>();

            for (int p = 0; p < componentsParent.Length; p++)
            {
                if (componentsParent[p].GetType() == typeof(Transform))
                {
                    var pos = componentsParent[p].transform.position;
                    var rot = componentsParent[p].transform.rotation.eulerAngles;

                    if (pos == null || rot == null)
                    {
                        failedGameObjects.Add(componentsParent[p].gameObject);
                    }
                }
            }

            for (int c = 0; c < componentsChild.Length; c++)
            {
                if (componentsChild[c].GetType() == typeof(Transform))
                {
                    var pos = componentsChild[c].transform.position;
                    var rot = componentsChild[c].transform.rotation.eulerAngles;

                    if (pos == null || rot == null)
                    {
                        failedGameObjects.Add(componentsChild[c].gameObject);
                    }
                }
            }

            if (failedGameObjects.Count == 0)
                return true;

            return false;
        }

        public bool CharacterCreateNose(GameObject selection, Animator animator, SkinnedMeshRenderer skinnedMeshRenderer, bool drawRays = false)
        {
            var model = AvatarCreateNose(selection, animator, skinnedMeshRenderer, drawRays);
            var childCount = model.transform.childCount;
            var nose = false;
            var earRight = false;
            var earLeft = false;

            for (int i = 0; i < childCount; i++)
            {
                if (model.transform.GetChild(i).name == "head")
                {
                    var modelHead = model.transform.GetChild(i);
                    var headChildCount = modelHead.transform.childCount;

                    for (int h = 0; h > headChildCount; h++)
                    {
                        var childName = modelHead.transform.GetChild(h).name;
                        if (childName.Contains("nose"))
                            nose = true;
                        if (childName.Contains("earRight"))
                            earRight = true;
                        if (childName.Contains("earLeft"))
                            earLeft = true;
                    }
                }
            }

            if (nose && earRight && earLeft)
                return true;

            return false;
        }
    }
}
