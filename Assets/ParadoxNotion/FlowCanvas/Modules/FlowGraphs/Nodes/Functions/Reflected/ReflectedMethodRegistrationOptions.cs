namespace FlowCanvas.Nodes
{

    public struct ReflectedMethodRegistrationOptions
    {
        public bool callable;
        public bool exposeParams;
        public int exposedParamsCount;
        //添加isNullable
        public bool isNullable;
    }
}