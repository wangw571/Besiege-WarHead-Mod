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
        public override string BesiegeVersion { get; } = "v0.27";
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
            /*.Properties(new BlockProperties().Key1("立即引爆", "k").Key2("保险开/关", "1")
                                             .CanBeDamaged(1f)
                                             .Slider("引爆延迟", 0, 10, 0.2f)
                                             )*/
            .Mass(0.5f)
            .IconOffset(new Icon(Vector3.one, Vector3.zero, new Vector3(-90f, 45f, 0f)))
            .ShowCollider(false)
            .AddingPoints(new List<AddingPoint> { new BasePoint(true, true) })
            .CompoundCollider(new List<ColliderComposite> { new ColliderComposite(new Vector3(0.8f, 0.8f, 1f), new Vector3(0.0f, 0.0f, 1.1f), new Vector3(0f, 0f, 0f)) })
            .NeededResources(new List<NeededResource>());


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

        private bool bomb;
        private bool fire;
        private bool safe;
        private bool press;
        private float fuse;
        private GameObject connected;
        private Collision col;


        public override void SafeAwake()
        {
            引爆 = AddKey("Detonated", //按键信息
                                 "Deto",           //名字
                                 KeyCode.K);       //默认按键

            保险 = AddKey("Safe", //按键信息
                                 "Safe",           //名字
                                 KeyCode.Alpha1);       //默认按键

            延迟 = AddSlider("Collision Explosion Delay",       //滑条信息
                                    "Delay",       //名字
                                    0.2f,            //默认值
                                    0f,          //最小值
                                    10f);           //最大值

            /*不聪明模式 = AddToggle("Disable Smart Attack",   //toggle信息
                                       "NoSA",       //名字
                                       false);             //默认状态*/
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
            if (bomb == true)
            {
                connected = Instantiate(FindObjectOfType<AddPiece>().blockTypes[23].gameObject);
                connected.transform.position = transform.position;
                Destroy(connected.GetComponent<Rigidbody>());
                connected.AddComponent<Rigidbody>();
                connected.gameObject.AddComponent<KillIfEditMode>();
                connected.GetComponent<ExplodeOnCollide>().radius = 24f;
                connected.GetComponent<FireTag>().Ignite();
                Destroy(gameObject);
            }


        }
        void OnCollisionEnter()
        {
            //if (connected == null) {connected =  this.gameObject.hingeJoint.connectedBody.gameObject;Debug.Log(connected); }
            if (safe == false /*&& Math.Abs(collision.relativeVelocity.x)+ Math.Abs(collision.relativeVelocity.y)+ Math.Abs(collision.relativeVelocity.z) > 3*/)
            {
                fire = true;
            }
            //Debug.Log(Math.Abs(col.relativeVelocity.x) + Math.Abs(col.relativeVelocity.y) + Math.Abs(col.relativeVelocity.z));
        }
        protected override void OnSimulateExit()
        {
            safe = false;
        }
    }
}
