using System;
using System.CodeDom;
using NMG.Core.Util;

namespace NMG.Core.Generator
{
    public class CodeGenerationHelper
    {
        public CodeCompileUnit GetCodeCompileUnit(string nameSpace, string className, params bool[] isCastle)
        {
            var codeCompileUnit = new CodeCompileUnit();
            var codeNamespace = new CodeNamespace(nameSpace);
            var codeTypeDeclaration = new CodeTypeDeclaration(className);
            if (null != isCastle && isCastle.Length > 0 && Convert.ToBoolean(isCastle[0]))
            {
                var codeAttributeDeclaration = new CodeAttributeDeclaration("ActiveRecord");
                codeTypeDeclaration.BaseTypes.Add(new CodeTypeReference("ActiveRecordValidationBase<" + className + ">"));
                codeTypeDeclaration.CustomAttributes.Add(codeAttributeDeclaration);
            }

            codeNamespace.Types.Add(codeTypeDeclaration);
            codeCompileUnit.Namespaces.Add(codeNamespace);
            return codeCompileUnit;
        }

        public CodeCompileUnit GetCodeCompileUnitWithInheritanceAndInterface(string nameSpace, string className, string inheritanceAndInterface, params bool[] isCastle)
        {
            var codeCompileUnit = GetCodeCompileUnit(nameSpace, className, isCastle);
            if (!string.IsNullOrEmpty(inheritanceAndInterface))
            {
                foreach (CodeNamespace ns in codeCompileUnit.Namespaces)
                {
                    foreach (CodeTypeDeclaration type in ns.Types)
                    {
                        foreach (var classOrInterface in inheritanceAndInterface.Split(','))
                            type.BaseTypes.Add(new CodeTypeReference(classOrInterface.Replace("<T>", "<" + className + ">").Trim()));
                    }
                }
            }
            return codeCompileUnit;
        }

        public CodeMemberProperty CreateProperty(Type type, string propertyName)
        {
            var codeMemberProperty = new CodeMemberProperty
                                         {
                                             Name = propertyName,
                                             HasGet = true,
                                             HasSet = true,
                                             Attributes = MemberAttributes.Public,
                                             Type = new CodeTypeReference(type)
                                         };

            string fieldName = propertyName.MakeFirstCharLowerCase();
            var codeFieldReferenceExpression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                                                                                fieldName);
            var returnStatement = new CodeMethodReturnStatement(codeFieldReferenceExpression);
            codeMemberProperty.GetStatements.Add(returnStatement);
            var assignStatement = new CodeAssignStatement(codeFieldReferenceExpression,
                                                          new CodePropertySetValueReferenceExpression());
            codeMemberProperty.SetStatements.Add(assignStatement);
            return codeMemberProperty;
        }

        public CodeMemberProperty CreateAutoProperty(Type type, string propertyName, bool fieldIsNull, bool useLazy = true)
        {
            bool setFieldAsNullable = fieldIsNull && IsNullable(type);
            if (setFieldAsNullable)
                type = typeof (Nullable<>).MakeGenericType(type);
            var codeMemberProperty = new CodeMemberProperty
                                         {
                                             Name = propertyName,
                                             HasGet = true,
                                             HasSet = true,
                                             Attributes = MemberAttributes.Public,
                                             Type = new CodeTypeReference(type)
                                         };
            if (!useLazy)
                codeMemberProperty.Attributes = codeMemberProperty.Attributes | MemberAttributes.Final;
            return codeMemberProperty;
        }

        public CodeMemberProperty CreateAutoProperty(string typeName, string propertyName, bool useLazy = true)
        {
            var codeMemberProperty = new CodeMemberProperty
                                         {
                                             Name = propertyName,
                                             HasGet = true,
                                             HasSet = true,
                                             Attributes = MemberAttributes.Public,
                                             Type = new CodeTypeReference(typeName)
                                         };
            if (!useLazy)
                codeMemberProperty.Attributes = codeMemberProperty.Attributes | MemberAttributes.Final;
            return codeMemberProperty;
        }

        // For Castle
        public CodeMemberProperty CreateAutoProperty(string typeName, string propertyName,
                                                     CodeAttributeDeclaration attributeArgument)
        {
            var codeMemberProperty = new CodeMemberProperty
                                         {
                                             Name = propertyName,
                                             HasGet = true,
                                             HasSet = true,
                                             Attributes = MemberAttributes.Public,
                                             Type = new CodeTypeReference(typeName)
                                         };

            codeMemberProperty.CustomAttributes.Add(attributeArgument);

            return codeMemberProperty;
        }

        public CodeMemberField CreateField(Type type, string fieldName)
        {
            return new CodeMemberField(type, fieldName);
        }

        // http://bytes.com/topic/c-sharp/answers/515498-typeof-check-nullability
        // Should probably move this elsewhere...
        private static bool IsNullable(Type type)
        {
            return type.IsValueType ||
                   (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>));
        }

        public CodeMemberProperty CreateAutoPropertyWithDataMemberAttribute(string type, string propertyName)
        {
            var attributes = new CodeAttributeDeclarationCollection { new CodeAttributeDeclaration("DataMember") };
            var codeMemberProperty = new CodeMemberProperty
            {
                Name = propertyName,
                HasGet = true,
                HasSet = true,
                CustomAttributes = attributes,
                Attributes = MemberAttributes.Public,
                Type = new CodeTypeReference(type)
            };
            return codeMemberProperty;
        }

        public string InstatiationObject(string foreignEntityCollectionType)
        {
            if (foreignEntityCollectionType.Contains("List"))
                return "List";
            if (foreignEntityCollectionType.Contains("Set"))
                return "HashedSet";
            return foreignEntityCollectionType;
        }
    }
}