using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class RegisterConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            return new CodeNode(aastNode, parent)
            {
                ByteToReturn = Generator.IdentifyRegister(aastNode.Token.Value)
            };
        }
    }
}
