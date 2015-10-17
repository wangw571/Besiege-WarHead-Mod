using System;
using System.Collections.Generic;
using spaar.ModLoader;
using TheGuysYouDespise;
using UnityEngine;

namespace Blocks
{
    public class WarheadMod : BlockMod
    {
        public override Version Version { get { return new Version("1.0"); } }
        public override string Name { get { return "WarheadMod)"; } }
        public override string DisplayName { get { return "Warhead Mod"; } }
        public override string BesiegeVersion { get { return "v0.11"; } }
        public override string Author { get { return "覅是"; } }
        protected Block Warhead = new Block()
                .ID(506)
                .TextureFile("Warhead.png")
                .BlockName("Warhead")
                .Obj(new List<Obj> { new Obj("Warhead.obj", new VisualOffset(Vector3.one, Vector3.zero, Vector3.zero)) })
                .Scripts(new Type[] { typeof(WarheadS) })
                .Properties(new BlockProperties().Key1("引爆", "k").Key2("保险开/关", "1")
                                                 .CanBeDamaged(Mathf.Infinity)
                                                 .Slider("引爆延迟", 0, 10, 0.2f)
                                                 )
                .Mass(0.75f)
                .IconOffset(new Icon(1f, new Vector3(0f, 0f, 0f), new Vector3(-90f, 45f, 0f)))//第一个float是图标缩放，五六七是我找的比较好的角度
                .ShowCollider(false)
                .AddingPoints(new List<AddingPoint> { new BasePoint(true, true) })
                .CompoundCollider(new List<ColliderComposite> { new ColliderComposite(new Vector3(0.7f,0.7f, 1.1f), new Vector3(0f, 0f, 0.6f), new Vector3(0f, 0f, 0f))})
                .NeededResources(new List<NeededResource> ()//需要的资源，例如音乐

            );
        public override void OnLoad()
        {
            LoadFancyBlock(Warhead);//加载该模块
        }
        public override void OnUnload() { }
    }


    public class WarheadS : BlockScript
    {
        private string key1;
        private string key2;
        private bool toggle;
        private int sliderValve;
        private bool 炸;
        private bool 保险;
        private bool 按下了;
        private GameObject connected;
        private Collision colsion;

        protected override void OnSimulateStart()
        {

            key1 = this.GetComponent<MyBlockInfo>().key1;
            key2 = this.GetComponent<MyBlockInfo>().key2;
            toggle = this.GetComponent<MyBlockInfo>().toggleModeEnabled;
            sliderValve = (int)this.GetComponent<MyBlockInfo>().sliderValue;
            炸 = false;
            按下了 = false;
            保险 = true;
        }
        protected override void OnSimulateFixedUpdate()
        {
            if (AddPiece.isSimulating)
            {
                Debug.Log(colsion);
                if (Input.GetKeyDown(key2) && 按下了 == false)
                {
                    按下了 = true;
                    if (保险 == true) { 保险 = false; }
                    else if (保险 == false) { 保险 = true; }
                }
                else { 按下了 = false; }
                    if (Input.GetKey(key1) && 保险 == false)
                {
                    炸 = true;
                }
                if (炸 == true)
                {
                    GameObject component = (GameObject)UnityEngine.Object.Instantiate(UnityEngine.Object.FindObjectOfType<AddPiece>().blockTypes[23].gameObject);
                    component.transform.position = this.transform.position;
                    UnityEngine.Object.Destroy(component.GetComponent<Rigidbody>());
                    component.AddComponent<Rigidbody>();
                    component.gameObject.AddComponent<KillIfEditMode>();
                    component.GetComponent<ExplodeOnCollide>().radius = 12f;
                    component.GetComponent<FireTag>().Ignite();
                    UnityEngine.Object.Destroy(this.gameObject);
                }

            }
            //Physics stuff

        }
        void OnCollisionEnter()
        {
            //if (connected == null) {connected =  this.gameObject.hingeJoint.connectedBody.gameObject;Debug.Log(connected); }
            if (保险 == false /*&& Math.Abs(collision.relativeVelocity.x)+ Math.Abs(collision.relativeVelocity.y)+ Math.Abs(collision.relativeVelocity.z) > 3*/)
            {
                炸 = true;
            }
            Debug.Log(Math.Abs(colsion.relativeVelocity.x) + Math.Abs(colsion.relativeVelocity.y) + Math.Abs(colsion.relativeVelocity.z));
        }
        protected override void OnSimulateExit()
        {
            保险 = false;
        }
    }


}
