using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace Aurora {
    //3 bits
    class ExportData {
        enum VertType {
            BYTE,
            UBYTE,
            SHORT,
            USHORT,
            INT,
            UINT,
            FLOAT
        }

        public static readonly uint FILE_HEADER = 0xBFC2D4F6;

        public static readonly ushort CHUNK_HEAD = 0x0001;

        public static readonly ushort CHUNK_MESH = 0x0002;
        public static readonly byte CHUNK_MESH_VERT = 0x01;
        public static readonly byte CHUNK_MESH_UV = 0x02;
        public static readonly byte CHUNK_MESH_NRM = 0x03;
        public static readonly byte CHUNK_MESH_DRAW_IDX = 0x04;

        public List<Mesh> meshes = new List<Mesh>();

        public MemoryStream encode() {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            writer.Write(FILE_HEADER);
            _writeChunk(writer, CHUNK_HEAD, _encodeChunkHead());

            for (int i = 0, n = meshes.Count; i < n; ++i) {
                _writeChunk(writer, CHUNK_MESH, _encodeChunkMesh(meshes[i]));
            }

            return ms;
        }

        private void _writeChunk(BinaryWriter writer, ushort chunk, MemoryStream chunkData) {
            var len = chunkData.Length;
            if (_writeChunkHeader(writer, chunk, len)) writer.Write(chunkData.GetBuffer(), 0, (int)len);
            chunkData.Close();
        }

        private void _writeChunk(BinaryWriter writer, byte chunk, MemoryStream chunkData) {
            var len = chunkData.Length;
            if (_writeChunkHeader(writer, chunk, len)) writer.Write(chunkData.GetBuffer(), 0, (int)len);
            chunkData.Close();
        }

        private MemoryStream _encodeChunkHead() {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            writeUint24(writer, 1);
            writer.Write((byte)0);

            return ms;
        }

        private MemoryStream _encodeChunkMesh(Mesh mesh) {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            _writeChunk(writer, CHUNK_MESH_VERT, _encodeMeshVertexData(mesh));
            _writeChunk(writer, CHUNK_MESH_UV, _encodeMeshVertexData(mesh));
            _writeChunk(writer, CHUNK_MESH_NRM, _encodeMeshVertexData(mesh));
            _writeChunk(writer, CHUNK_MESH_DRAW_IDX, _encodeMeshDrawIndexData(mesh));

            return ms;
        }

        private static MemoryStream _encodeMeshVertexData(Mesh mesh) {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            var vertices = mesh.vertices;

            writer.Write((byte)((int)VertType.FLOAT << 2 | 3));
            foreach (var v in vertices) {
                writer.Write((float)v.x);
                writer.Write((float)v.y);
                writer.Write((float)v.z);
            }

            return ms;
        }

        private static MemoryStream _encodeMeshNormalData(Mesh mesh) {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            var normals = mesh.normals;

            writer.Write((byte)((int)VertType.FLOAT << 2 | 3));
            foreach (var v in normals) {
                writer.Write((float)v.x);
                writer.Write((float)v.y);
                writer.Write((float)v.z);
            }

            return ms;
        }

        private static MemoryStream _encodeMeshUVData(Mesh mesh) {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            var uv = mesh.uv;

            writer.Write((byte)((int)VertType.FLOAT << 2 | 2));
            foreach (var v in uv) {
                writer.Write((float)v.x);
                writer.Write((float)v.y);
            }

            return ms;
        }

        private static MemoryStream _encodeMeshDrawIndexData(Mesh mesh) {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            var triangles = mesh.triangles;

            var lenType = _calcLenType(mesh.vertices.Length);

            writer.Write((byte)lenType);
            if (lenType == 1) {
                foreach (var i in triangles) writer.Write((byte)i);
            } else if (lenType == 2) {
                foreach (var i in triangles) writer.Write((ushort)i);
            } else if (lenType == 3) {
                foreach (var i in triangles) writer.Write((uint)i);
            }
            
            return ms;
        }

        private static bool _writeChunkHeader(BinaryWriter writer, ushort chunk, long length) {
            chunk = (ushort)_generateChunkID(chunk, length);
            writer.Write(chunk);
            _writeChunkLength(writer, chunk, length);
            return length > 0;
        }

        private static bool _writeChunkHeader(BinaryWriter writer, byte chunk, long length) {
            chunk = (byte)_generateChunkID(chunk, length);
            writer.Write(chunk);
            _writeChunkLength(writer, chunk, length);
            return length > 0;
        }

        private static uint _calcLenType(long length) {
            if (length == 0) {
                return 0;
            } else if (length <= 0xFF) {
                return 1;
            } else if (length <= 0xFFFF) {
                return 2;
            } else {
                return 3;
            }
        }

        private static uint _generateChunkID(uint chunk, long length) {
            return (chunk << 2) | _calcLenType(length);
        }

        private static void _writeChunkLength(BinaryWriter writer, uint chunk, long length) {
            var type = chunk & 0x3;
            if (type == 1) {
                writer.Write((byte)length);
            } else if (type == 2) {
                writer.Write((ushort)length);
            } else if (type == 3) {
                writer.Write((uint)length);
            }
        }

        public static void writeUint24(BinaryWriter writer, int value) {
            writer.Write((byte)(value & 0xFF));
            writer.Write((byte)(value >> 8 & 0xFF));
            writer.Write((byte)(value >> 16 & 0xFF));
        }
    }
}
