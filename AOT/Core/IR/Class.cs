// 
// (C) 2006-2007 The SharpOS Project Team (http://www.sharpos.org)
//
// Authors:
//	Mircea-Cristian Racasan <darx_kies@gmx.net>
//
// Licensed under the terms of the GNU GPL v3,
//  with Classpath Linking Exception for Libraries
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using SharpOS.AOT.IR;
using SharpOS.AOT.IR.Instructions;
using SharpOS.AOT.IR.Operands;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Metadata;

namespace SharpOS.AOT.IR {
	public class Class : IEnumerable<Method> {
		/// <summary>
		/// Initializes a new instance of the <see cref="Class"/> class.
		/// </summary>
		/// <param name="engine">The engine.</param>
		/// <param name="classDefinition">The class definition.</param>
		public Class (Engine engine, TypeDefinition classDefinition)
		{
			this.engine = engine;
			this.classDefinition = classDefinition;
		}

		private Engine engine = null;
		private TypeDefinition classDefinition = null;

		/// <summary>
		/// Gets the class definition.
		/// </summary>
		/// <value>The class definition.</value>
		public TypeDefinition ClassDefinition {
			get {
				return this.classDefinition;
			}
		}

		public string TypeFullName {
			get {
				foreach (CustomAttribute attribute in classDefinition.CustomAttributes) {
					if (!attribute.Constructor.DeclaringType.FullName.Equals (typeof (SharpOS.AOT.Attributes.TargetNamespaceAttribute).ToString ()))
							continue;

					return attribute.ConstructorParameters [0].ToString () + "." + this.classDefinition.Name;
				}

				return this.classDefinition.FullName;
			}
		}

		/// <summary>
		/// Adds the specified method.
		/// </summary>
		/// <param name="method">The method.</param>
		public void Add (Method method)
		{
			this.methods.Add (method);
		}

		private List<Method> methods = new List<Method> ();

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
		/// </returns>
		IEnumerator<Method> IEnumerable<Method>.GetEnumerator ()
		{
			foreach (Method method in this.methods)
				yield return method;
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IEnumerable<Method>) this).GetEnumerator ();
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </returns>
		public override string ToString ()
		{
			if (this.classDefinition != null)
				return this.classDefinition.FullName;

			return base.ToString ();
		}

		/// <summary>
		/// Gets the type of the field.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public InternalType GetFieldType (string value)
		{
			foreach (FieldDefinition field in this.classDefinition.Fields) {
				if (field.Name.Equals (value))
					return this.engine.GetInternalType (field.FieldType.FullName);
			}

			return InternalType.NotSet;
		}

		/// <summary>
		/// Gets the type of the internal.
		/// </summary>
		/// <returns></returns>
		public InternalType GetInternalType ()
		{
			if (this.classDefinition.IsEnum) {
				foreach (FieldDefinition field in this.classDefinition.Fields) {
					if ((field.Attributes & FieldAttributes.RTSpecialName) != 0)
						return this.engine.GetInternalType (field.FieldType.FullName);
				}

			} else if (this.classDefinition.IsValueType)
				return Operands.InternalType.ValueType;

			else if (this.classDefinition.IsClass)
				return Operands.InternalType.O;

			return InternalType.NotSet;
		}

		/// <summary>
		/// Gets the size.
		/// </summary>
		/// <returns></returns>
		public int GetSize ()
		{
			int result = -1;

			if (this.classDefinition.IsEnum) {
				result = 0;

				foreach (FieldDefinition field in this.classDefinition.Fields) {
					if ((field.Attributes & FieldAttributes.RTSpecialName) != 0) {
						result = this.engine.GetTypeSize (field.FieldType.FullName);
						break;
					}
				}

			} else if (this.classDefinition.IsValueType) {
				if ((this.classDefinition.Attributes & TypeAttributes.ExplicitLayout) != 0) {
					result = 0;

					foreach (FieldDefinition field in this.classDefinition.Fields) {
						if ((field as FieldDefinition).IsStatic)
							continue;

						int value = (int) (field.Offset + this.engine.GetTypeSize (field.FieldType.FullName));

						if (value > result)
							result = value;
					}

				} else {
					result = 0;

					foreach (FieldReference field in this.classDefinition.Fields) {
						if ((field as FieldDefinition).IsStatic)
							continue;

						result += this.engine.GetFieldSize (field.FieldType.FullName);
					}
				}
			} 

			return result;
		}
	}
}
