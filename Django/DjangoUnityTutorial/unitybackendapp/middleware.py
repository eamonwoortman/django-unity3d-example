# Copyright (c) 2015 Eamon Woortman
#
# Permission is hereby granted, free of charge, to any person
# obtaining a copy of this software and associated documentation
# files (the "Software"), to deal in the Software without
# restriction, including without limitation the rights to use,
# copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the
# Software is furnished to do so, subject to the following
# conditions:
#
# The above copyright notice and this permission notice shall be
# included in all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
# EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
# OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
# NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
# HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
# WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
# FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
# OTHER DEALINGS IN THE SOFTWARE.

class Unity3DMiddleware(object):
    """
    An Unity3D middleware which replaces the response status code with 200 
    and adds a REAL_STATUS_CODE header containing the original status code. 
    It also replaces the request method with the one specified in the 'UNITY_METHOD' 
    header field. 
    These modifications are necessary if you want to work with Unity's WWW class and 
    still keep the original Django RestFramework conventions.
    """
    def process_request(self, request):
        """
        Replaces the request method with the method defined in the 
        UNITY_METHOD header field(if specified).
        """
        meta = getattr(request, 'META', False)
        if 'HTTP_UNITY_METHOD' in meta:
            method = meta['HTTP_UNITY_METHOD']
            if method in ['POST', 'DELETE', 'UPDATE', 'GET']:
                request.method = method
            
    def process_response(self, request, response):
        """
        Replaces the status code with 200 and adds a 'REAL_STATUS' header field with the 
        original status code and text.
        """
        if request.is_ajax():
            response["REAL_STATUS"] = '%s %s' % (response.status_code, response.status_text)
            response.status_code = 200
        return response