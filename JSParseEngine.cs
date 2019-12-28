using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LerioCN
{
    /// <summary>
    /// JavaScript解析引擎
    /// </summary>
    public class JSParseEngine
    {
        /// <summary>
        /// 方法类型
        /// </summary>
        private enum MethodType
        {
            /// <summary>
            /// 通过VAR定义
            /// </summary>
            BYVAR,
            /// <summary>
            /// 通用方式
            /// </summary>
            COMMON
        }

        /// <summary>
        /// js文件内容
        /// </summary>
        private string _jsContent;

        /// <summary>
        /// 解析结果
        /// </summary>
        private List<string> _lstMethodName;

        /// <summary>
        /// 获取JS方法集合
        /// </summary>
        /// <returns></returns>
        public List<string> GetMethodNameList()
        {
            return _lstMethodName.Select(s => s).ToList();
        }

        /// <summary>
        /// 解析方法内容
        /// </summary>
        private Dictionary<string, string> _dictMethodContent;

        /// <summary>
        /// 获取方法体
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetMethodContentDict()
        {
            return _dictMethodContent.ToDictionary(s => s.Key, s => s.Value);
        }

        /// <summary>
        /// 构造执行
        /// </summary>
        /// <param name="js">内容或者文件名</param>
        /// <param name="isFile">是否为文件路径，默认为内容文本</param>
        public JSParseEngine(string js, bool isFile = false)
        {
            if (isFile)
            {
                if (File.Exists(js) == false)
                {
                    throw new System.Exception($"File is not exist:{js}");
                }

                _jsContent = File.ReadAllText(js);
            }
            else
            {
                _jsContent = js;
            }


        }

        /// <summary>
        /// 解析方法名称
        /// </summary>
        private void ParseMethodName()
        {
            _lstMethodName = new List<string>();

            string funcName = string.Empty;
            //提取所有的方法名
            string[] jsSplitArr = _jsContent
                .Replace("/", "")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ")
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < jsSplitArr.Length; i++)
            {
                string tmp = jsSplitArr[i];
                if (tmp.ToLower() == "function")
                {
                    if (i > 0)
                    {
                        if (i - 1 >= 0)
                        {
                            if (jsSplitArr[i - 1] == "=")
                            {
                                if (i - 2 >= 0)
                                {
                                    funcName = jsSplitArr[i - 2];
                                    if (!_lstMethodName.Contains(funcName))
                                    {
                                        _lstMethodName.Add(funcName);
                                    }
                                }
                            }
                            else
                            {
                                if (i + 2 <= jsSplitArr.Length - 1 && jsSplitArr[i + 2] == "(")
                                {
                                    funcName = jsSplitArr[i + 1];
                                    if (!_lstMethodName.Contains(funcName))
                                    {
                                        _lstMethodName.Add(funcName);
                                    }
                                }
                            }
                        }

                    }
                    else if (i <= jsSplitArr.Length - 1)
                    {
                        funcName = jsSplitArr[i + 1];
                        if (!_lstMethodName.Contains(funcName))
                        {
                            _lstMethodName.Add(funcName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 解析方法内容
        /// </summary>
        private void ParseMethodContent()
        {
            _dictMethodContent = new Dictionary<string, string>();
            if (_lstMethodName.Count > 0)
            {
                foreach (var method in _lstMethodName)
                {
                    string func = $"{method} = function";
                    int startFunc = _jsContent.IndexOf(func);
                    if (startFunc < 0)
                    {
                        continue;
                    }

                    int startLeft = _jsContent.IndexOf('{', startFunc);
                    if (startLeft > startFunc)
                    {
                        int count = 1;

                        int tmpIdx = _jsContent.IndexOfAny(new char[] { '}', '{' }, startLeft + 1);
                        while (tmpIdx >= startLeft)
                        {
                            string tmpChar = _jsContent.Substring(tmpIdx, 1);
                            if (tmpChar == "}")
                            {
                                count--;
                            }
                            else if (tmpChar == "{")
                            {
                                count++;
                            }

                            if (count == 0)
                            {
                                break;
                            }

                            startLeft = tmpIdx;
                            tmpIdx = _jsContent.IndexOfAny(new char[] { '}', '{' }, tmpIdx + 1);
                        }

                        if (tmpIdx > startFunc)
                        {
                            string funcContent = _jsContent.Substring(startFunc, tmpIdx - startFunc + 1);
                            if (!_dictMethodContent.ContainsKey(method))
                            {
                                _dictMethodContent.Add(method, funcContent);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 解析内容
        /// </summary>
        /// <returns></returns>
        public bool Parse()
        {
            //解析方法名
            ParseMethodName();
            //解析方法体
            ParseMethodContent();
            return true;
        }
    }
}

/*
JSParseEngine jSParseEngine = new JSParseEngine(textBox1.Text);
jSParseEngine.Parse();

foreach (var kvp in jSParseEngine.GetMethodContentDict())
{
    textBox2.AppendText(kvp.Key);
    textBox2.AppendText(Environment.NewLine);
    textBox2.AppendText(Environment.NewLine);
}
*/
