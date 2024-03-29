﻿#region netDxf library licensed under the MIT License, Copyright © 2009-2021 Daniel Carvajal (haplokuon@gmail.com)
// 
//                        netDxf library
// Copyright © 2021 Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the “Software”), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a polyface mesh <see cref="EntityObject">entity</see>.
    /// </summary>
    /// <remarks>
    /// The maximum number of vertexes and faces that a PolyfaceMesh can have is short.MaxValue = 32767.
    /// </remarks>
    public class PolyfaceMesh :
        EntityObject
    {
        #region private fields

        private readonly List<PolyfaceMeshFace> faces;
        private readonly List<PolyfaceMeshVertex> vertexes;
        private readonly PolylineTypeFlags flags;
        private readonly EndSequence endSequence;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>PolyfaceMesh</c> class.
        /// </summary>
        /// <param name="vertexes">Polyface mesh <see cref="PolyfaceMeshVertex">vertex</see> list.</param>
        /// <param name="faces">Polyface mesh <see cref="PolyfaceMeshFace">faces</see> list.</param>
        public PolyfaceMesh(IEnumerable<PolyfaceMeshVertex> vertexes, IEnumerable<PolyfaceMeshFace> faces)
            : base(EntityType.PolyfaceMesh, DxfObjectCode.Polyline)
        {
            this.flags = PolylineTypeFlags.PolyfaceMesh;
            if (vertexes == null)
                throw new ArgumentNullException(nameof(vertexes));
            this.vertexes = new List<PolyfaceMeshVertex>(vertexes);
            if (this.vertexes.Count < 3)
                throw new ArgumentOutOfRangeException(nameof(vertexes), this.vertexes.Count, "The polyface mesh faces list requires at least three points.");

            if (faces == null)
                throw new ArgumentNullException(nameof(vertexes));
            this.faces = new List<PolyfaceMeshFace>(faces);
            if (this.faces.Count < 1)
                throw new ArgumentOutOfRangeException(nameof(vertexes), this.faces.Count, "The polyface mesh faces list requires at least one face.");

            this.endSequence = new EndSequence(this);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the polyface mesh <see cref="PolyfaceMeshVertex">vertexes</see>.
        /// </summary>
        public IReadOnlyList<PolyfaceMeshVertex> Vertexes
        {
            get { return this.vertexes; }
        }

        /// <summary>
        /// Gets or sets the polyface mesh <see cref="PolyfaceMeshFace">faces</see>.
        /// </summary>
        public IReadOnlyList<PolyfaceMeshFace> Faces
        {
            get { return this.faces; }
        }

        #endregion

        #region internal properties

        /// <summary>
        /// Gets the polyface mesh flag type.
        /// </summary>
        internal PolylineTypeFlags Flags
        {
            get { return this.flags; }
        }

        /// <summary>
        /// Gets the end vertex sequence.
        /// </summary>
        internal EndSequence EndSequence
        {
            get { return this.endSequence; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Assigns a handle to the object based in a integer counter.
        /// </summary>
        /// <param name="entityNumber">Number to assign.</param>
        /// <returns>Next available entity number.</returns>
        /// <remarks>
        /// Some objects might consume more than one, is, for example, the case of polylines that will assign
        /// automatically a handle to its vertexes. The entity number will be converted to an hexadecimal number.
        /// </remarks>
        internal override long AssignHandle(long entityNumber)
        {
            entityNumber = this.endSequence.AssignHandle(entityNumber);
            foreach (PolyfaceMeshVertex v in this.vertexes)
            {
                entityNumber = v.AssignHandle(entityNumber);
            }
            foreach (PolyfaceMeshFace f in this.faces)
            {
                entityNumber = f.AssignHandle(entityNumber);
            }
            return base.AssignHandle(entityNumber);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Decompose the actual polyface mesh faces in <see cref="Point">points</see> (one vertex polyface mesh face),
        /// <see cref="Line">lines</see> (two vertexes polyface mesh face) and <see cref="Face3d">3d faces</see> (three or four vertexes polyface mesh face).
        /// </summary>
        /// <returns>A list of <see cref="Face3d">3d faces</see> that made up the polyface mesh.</returns>
        public List<EntityObject> Explode()
        {
            List<EntityObject> entities = new List<EntityObject>();

            foreach (PolyfaceMeshFace face in this.Faces)
            {
                if (face.VertexIndexes.Count == 1)
                {
                    Point point = new Point
                    {
                        Layer = (Layer) this.Layer.Clone(),
                        Linetype = (Linetype) this.Linetype.Clone(),
                        Color = (AciColor) this.Color.Clone(),
                        Lineweight = this.Lineweight,
                        Transparency = (Transparency) this.Transparency.Clone(),
                        LinetypeScale = this.LinetypeScale,
                        Normal = this.Normal,
                        Position = this.Vertexes[Math.Abs(face.VertexIndexes[0]) - 1].Position,
                    };
                    entities.Add(point);
                    continue;
                }
                if (face.VertexIndexes.Count == 2)
                {
                    Line line = new Line
                    {
                        Layer = (Layer) this.Layer.Clone(),
                        Linetype = (Linetype) this.Linetype.Clone(),
                        Color = (AciColor) this.Color.Clone(),
                        Lineweight = this.Lineweight,
                        Transparency = (Transparency) this.Transparency.Clone(),
                        LinetypeScale = this.LinetypeScale,
                        Normal = this.Normal,
                        StartPoint = this.Vertexes[Math.Abs(face.VertexIndexes[0]) - 1].Position,
                        EndPoint = this.Vertexes[Math.Abs(face.VertexIndexes[1]) - 1].Position,
                    };
                    entities.Add(line);
                    continue;
                }

                Face3dEdgeFlags edgeVisibility = Face3dEdgeFlags.None;

                short indexV1 = face.VertexIndexes[0];
                short indexV2 = face.VertexIndexes[1];
                short indexV3 = face.VertexIndexes[2];
                // Polyface mesh faces are made of 3 or 4 vertexes, we will repeat the third vertex if the number of face vertexes is three
                int indexV4 = face.VertexIndexes.Count == 3 ? face.VertexIndexes[2] : face.VertexIndexes[3];

                if (indexV1 < 0)
                {
                    edgeVisibility = edgeVisibility | Face3dEdgeFlags.First;
                }

                if (indexV2 < 0)
                {
                    edgeVisibility = edgeVisibility | Face3dEdgeFlags.Second;
                }

                if (indexV3 < 0)
                {
                    edgeVisibility = edgeVisibility | Face3dEdgeFlags.Third;
                }

                if (indexV4 < 0)
                {
                    edgeVisibility = edgeVisibility | Face3dEdgeFlags.Fourth;
                }

                Vector3 v1 = this.Vertexes[Math.Abs(indexV1) - 1].Position;
                Vector3 v2 = this.Vertexes[Math.Abs(indexV2) - 1].Position;
                Vector3 v3 = this.Vertexes[Math.Abs(indexV3) - 1].Position;
                Vector3 v4 = this.Vertexes[Math.Abs(indexV4) - 1].Position;

                Face3d face3d = new Face3d
                {
                    Layer = (Layer) this.Layer.Clone(),
                    Linetype = (Linetype) this.Linetype.Clone(),
                    Color = (AciColor) this.Color.Clone(),
                    Lineweight = this.Lineweight,
                    Transparency = (Transparency) this.Transparency.Clone(),
                    LinetypeScale = this.LinetypeScale,
                    Normal = this.Normal,
                    FirstVertex = v1,
                    SecondVertex = v2,
                    ThirdVertex = v3,
                    FourthVertex = v4,
                    EdgeFlags = edgeVisibility,
                };

                entities.Add(face3d);
            }

            return entities;
        }

        #endregion

        #region overrides

        /// <summary>
        /// Moves, scales, and/or rotates the current entity given a 3x3 transformation matrix and a translation vector.
        /// </summary>
        /// <param name="transformation">Transformation matrix.</param>
        /// <param name="translation">Translation vector.</param>
        /// <remarks>Matrix3 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
        public override void TransformBy(Matrix3 transformation, Vector3 translation)
        {
            foreach (PolyfaceMeshVertex point in this.Vertexes)
            {
                point.Position = transformation * point.Position + translation;
            }

            Vector3 newNormal = transformation * this.Normal;
            if (Vector3.Equals(Vector3.Zero, newNormal))
            {
                newNormal = this.Normal;
            }
            this.Normal = newNormal;
        }

        /// <summary>
        /// Creates a new PolyfaceMesh that is a copy of the current instance.
        /// </summary>
        /// <returns>A new PolyfaceMesh that is a copy of this instance.</returns>
        public override object Clone()
        {
            List<PolyfaceMeshVertex> copyVertexes = new List<PolyfaceMeshVertex>();
            foreach (PolyfaceMeshVertex vertex in this.vertexes)
            {
                copyVertexes.Add((PolyfaceMeshVertex) vertex.Clone());
            }
            List<PolyfaceMeshFace> copyFaces = new List<PolyfaceMeshFace>();
            foreach (PolyfaceMeshFace face in this.faces)
            {
                copyFaces.Add((PolyfaceMeshFace) face.Clone());
            }

            PolyfaceMesh entity = new PolyfaceMesh(copyVertexes, copyFaces)
            {
                //EntityObject properties
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) this.Linetype.Clone(),
                Color = (AciColor) this.Color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal,
                IsVisible = this.IsVisible,
                //PolyfaceMesh properties
            };

            foreach (XData data in this.XData.Values)
            {
                entity.XData.Add((XData) data.Clone());
            }

            return entity;
        }

        #endregion
    }
}