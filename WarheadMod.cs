using System;
using UnityEngine;
using System.Collections.Generic;
using Blocks;
using TheGuysYouDespise;
using System.Collections;

namespace WarheadBlock
{

    public class WarheadBlockMod : BlockMod
    {
        public override string Name { get; } = "Warhead";
        public override string DisplayName { get; } = "Warhead";
        public override string Author { get; } = "覅是";
        public override Version Version { get; } = new Version(1, 0, 1, 0);
        public override string BesiegeVersion { get; } = "v0.32";
        public override bool CanBeUnloaded { get; } = false;
        public override bool Preload { get; } = false;
        protected Block Warhead = new Block()
            .ID(540)
            .BlockName("Warhead")
            .Obj(new List<Obj> { new Obj("Warhead.obj", //Obj
                                         "Warhead.png", //贴图
                                         new VisualOffset(new Vector3(1f, 1f, 1f), //Scale
                                                          new Vector3(0f, 0f, 0f), //Position
                                                          new Vector3(0f, 0f, 0f)))//Rotation
            })
            .Components(new Type[] {
                                    typeof(WarheadblockS),
            })
            .Properties(new BlockProperties().SearchKeywords(new string[] {
                                                             "Bomb",
                                                             "炮弹",
                                                             "War",
                                                             "Weapon",
                                                             "Ammo",
                                                             "炸"
                                             })
            )
            .Mass(0.5f)
            .IconOffset(new Icon(Vector3.one, Vector3.zero, new Vector3(-90f, 45f, 0f)))
            .ShowCollider(!true)
            .AddingPoints(new List<AddingPoint> { new BasePoint(true, true) })
            .CompoundCollider(new List<ColliderComposite> { new ColliderComposite(new Vector3(0.8f, 0.8f, 1.1f), new Vector3(0.0f, 0.0f, 0.55f), new Vector3(0f, 0f, 0f)) })
            .NeededResources(new List<NeededResource>(){ new NeededResource(ResourceType.Audio, "missleLaunch.ogg"), new NeededResource(ResourceType.Mesh, "Motion Stopper Bubble.obj"), new NeededResource(ResourceType.Texture, "Bubble Texture.png") });


        public override void OnLoad()
        {
            LoadBlock(Warhead);
        }
    }


    public class WarheadblockS : BlockScript
    {
        protected MKey 引爆;
        protected MKey 保险;
        protected MSlider 延迟;
        protected MMenu 模式;

        private bool bomb;
        private bool hasFrozen;
        private bool fire;
        private bool safe;
        private bool press;
        private float fuse;
        private GameObject connected;
        private Collision col;
        private Texture NanoTexture;


        public override void SafeAwake()
        {
            引爆 = AddKey("Detonated", //按键信息
                                 "Deto",           //名字
                                 KeyCode.K);       //默认按键

            保险 = AddKey("Safety", //按键信息
                                 "Safe",           //名字
                                 KeyCode.Alpha1);       //默认按键

            延迟 = AddSlider("Collision Explosion Delay",       //滑条信息
                                    "Delay",       //名字
                                    0.2f,            //默认值
                                    0f,          //最小值
                                    10f);           //最大值
            模式 = AddMenu("Modes", 0, new List<string>() { "Bomb", "Motion Stopper", "DestroyImmediate" });

        }

        protected virtual IEnumerator UpdateMapper()
        {
            if (BlockMapper.CurrentInstance == null)
                yield break;
            while (Input.GetMouseButton(0))
                yield return null;
            BlockMapper.CurrentInstance.Copy();
            BlockMapper.CurrentInstance.Paste();
            yield break;
        }
        public override void OnSave(XDataHolder data)
        {
            SaveMapperValues(data);
        }
        public override void OnLoad(XDataHolder data)
        {
            LoadMapperValues(data);
            if (data.WasSimulationStarted) return;
        }

        protected override void OnSimulateStart()
        {
            bomb = false;
            fire = false;
            safe = true;
            press = false;
            fuse = 0;
            NanoTexture = resources["Bubble Texture.png"].texture;
        }
        protected override void OnSimulateUpdate()
        {
            //Debug.Log(col);
            if (保险.IsReleased && press == false)
            {
                press = true;
                safe = !safe;
            }
            else { press = false; }
            if (引爆.IsPressed && safe == false)
            {
                bomb = true;
            }
        }
        protected override void OnSimulateFixedUpdate()
        {
            if (fire == true)
            {
                fuse += Time.fixedDeltaTime;
                if (fuse > 延迟.Value)
                {
                    bomb = true;
                }
            }
            if (bomb)
            {
                if (模式.Value == 0)
                {
                    connected = Instantiate(PrefabMaster.BlockPrefabs[23].gameObject);
                    connected.transform.position = transform.position;
                    Destroy(connected.GetComponent<Rigidbody>());
                    Destroy(connected.GetComponent<Collider>());
                    connected.gameObject.AddComponent<KillIfEditMode>();
                    connected.GetComponent<ExplodeOnCollideBlock>().radius = 24f;
                    connected.GetComponent<ExplodeOnCollideBlock>().Explodey();
                    Destroy(gameObject);
                }
                else if (模式.Value == 1 && !hasFrozen)
                {
                    hasFrozen = true;
                    foreach (GameObject CloseEnoughToStop in FindObjectsOfType<GameObject>())
                    {
                        try
                        {
                            if (Vector3.Distance(CloseEnoughToStop.transform.position, this.transform.position) < 9)
                            {
                                CloseEnoughToStop.GetComponent<Rigidbody>().mass *= 200;
                                CloseEnoughToStop.GetComponent<Rigidbody>().drag = 400;
                                CloseEnoughToStop.GetComponent<Rigidbody>().angularDrag = 400;
                                if (CloseEnoughToStop.GetComponent<MyBlockInfo>().blockName == "BOMB")
                                {
                                    Destroy(CloseEnoughToStop.GetComponent<ExplodeOnCollideBlock>());
                                }
                                else if (CloseEnoughToStop.GetComponent<MyBlockInfo>().blockName == "GRENADE")
                                {
                                    Destroy(CloseEnoughToStop.GetComponent<ControllableBomb>());
                                }
                            }
                        }
                        catch { }
                    }
                    Vector3 currentLocation = new Vector3(this.transform.position.x, this.transform.position.y - 8, this.transform.position.z); ;
                    GameObject CyanCoverBall = new GameObject("Motion Stopper Bubble", new Type[] { typeof(MeshRenderer), typeof(MeshFilter) });
                    Destroy(CyanCoverBall.GetComponent<Rigidbody>());
                    CyanCoverBall.GetComponent<MeshFilter>().sharedMesh = resources["Motion Stopper Bubble.obj"].mesh;
                    CyanCoverBall.transform.position = currentLocation;
                    CyanCoverBall.GetComponent<MeshRenderer>().material.shader = Shader.Find("Transparent/Diffuse");//Maybe "Custom/TranspDiffuseRim"
                    CyanCoverBall.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", NanoTexture);
                    CyanCoverBall.AddComponent<MeshCollider>();
                    CyanCoverBall.GetComponent<MeshCollider>().sharedMesh = resources["Motion Stopper Bubble.obj"].mesh;
                    CyanCoverBall.GetComponent<MeshCollider>().enabled = true;
                    CyanCoverBall.GetComponent<MeshCollider>().convex = true;
                    CyanCoverBall.GetComponent<MeshCollider>().isTrigger = true;
                    CyanCoverBall.transform.SetParent(this.transform);
                    CyanCoverBall.AddComponent<StopMotionBubble>();
                    this.GetComponent<Rigidbody>().isKinematic = true;
                    //Destroy(Trail);
                }
                
            }
        }
        protected override void OnSimulateCollisionEnter(Collision coll)
        {
            //if (connected == null) {connected =  this.gameObject.hingeJoint.connectedBody.gameObject;Debug.Log(connected); }
            if (safe == false /*&& Math.Abs(collision.relativeVelocity.x)+ Math.Abs(collision.relativeVelocity.y)+ Math.Abs(collision.relativeVelocity.z) > 3*/)
            {
                fire = true;
            }
            if (模式.Value == 2 && fire  && coll.rigidbody)
            {
                Destroy(coll.gameObject);
            }
            //Debug.Log(Math.Abs(col.relativeVelocity.x) + Math.Abs(col.relativeVelocity.y) + Math.Abs(col.relativeVelocity.z));
        }
        protected override void OnSimulateExit()
        {
            safe = false;
        }
    }

    public class StopMotionBubble : MonoBehaviour
    {
        private float SizeMutiplier = 0.3f;
        private Vector3 startPos;
        public Vector2 OffsetValue;
        void Start()
        {
            startPos = this.transform.position;
            OffsetValue = new Vector2(UnityEngine.Random.Range(-0.002f, 0.002f), UnityEngine.Random.Range(-0.002f, 0.002f));
        }
        void FixedUpdate()
        {
            OffsetValue += new Vector2(UnityEngine.Random.Range(-0.0001f, 0.0001f), UnityEngine.Random.Range(-0.0001f, 0.0001f));
            if (StatMaster.isSimulating)
            {
                this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y + 0.05f, this.transform.eulerAngles.z);
                this.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", this.GetComponent<MeshRenderer>().material.mainTextureOffset + OffsetValue);
                this.transform.position = startPos + Vector3.up * (16.30472f - this.transform.localScale.y) / 2;
                this.transform.localScale *= 1 + SizeMutiplier;
                SizeMutiplier *= 0.9f;
            }
            else { Destroy(this); }

            /*if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                this.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(this.GetComponent<MeshRenderer>().material.mainTextureOffset.x + 0.005f, this.GetComponent<MeshRenderer>().material.mainTextureOffset.y));
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                this.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(this.GetComponent<MeshRenderer>().material.mainTextureOffset.x, this.GetComponent<MeshRenderer>().material.mainTextureOffset.y + 0.005f));
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                this.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(this.GetComponent<MeshRenderer>().material.mainTextureOffset.x - 0.005f, this.GetComponent<MeshRenderer>().material.mainTextureOffset.y ));
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                this.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(this.GetComponent<MeshRenderer>().material.mainTextureOffset.x, this.GetComponent<MeshRenderer>().material.mainTextureOffset.y - 0.005f));
            }
            if (Input.GetKeyDown("5"))
            {
                Debug.Log(this.GetComponent<MeshRenderer>().material.mainTextureOffset.x + "a " + this.GetComponent<MeshRenderer>().material.mainTextureOffset.y);
            }*/

        }
        void OnTriggerEnter(Collider coll)
        {
            if (coll.attachedRigidbody != null && StatMaster.isSimulating)
            {
                coll.attachedRigidbody.isKinematic = true;
            }
            foreach (Renderer M in coll.gameObject.GetComponentsInChildren<Renderer>())
            {
                if (!M.name.Contains("FloorBig"))
                {
                    M.material.color = Color.cyan;
                }
            }
        }
        void OnTriggerStay(Collider collision)
        {
            if (collision.attachedRigidbody != null && StatMaster.isSimulating)
            {
                collision.attachedRigidbody.mass += 33;
                collision.attachedRigidbody.drag += 20;
                collision.attachedRigidbody.angularDrag += 40;
                collision.attachedRigidbody.velocity *= 0.8f;
                collision.attachedRigidbody.angularVelocity *= 0.8f;
                collision.attachedRigidbody.useGravity = false;

            }
        }
        void OnTriggerExit(Collider collision)
        {
            if (collision.attachedRigidbody != null && StatMaster.isSimulating)
            {
                collision.attachedRigidbody.mass = collision.attachedRigidbody.mass % 33 + 0.1f;
                collision.attachedRigidbody.useGravity = true;
                collision.attachedRigidbody.drag = collision.attachedRigidbody.drag % 20;
                collision.attachedRigidbody.angularDrag = collision.attachedRigidbody.angularDrag % 40;
            }
        }

    }

}
