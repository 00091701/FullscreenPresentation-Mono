#!/usr/bin/env python
#
# Copyright 2007 Google Inc.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#
import webapp2
import sys
import os.path

# import path for google io 2012 slides
sys.path.append(os.path.join(os.path.join(os.path.join('.', 'io-2012-slides'), 'scripts'), 'md'))

# import path for markdown lib
sys.path.append(os.path.join(os.path.join('.', 'lib'), 'python-markdown'))

import codecs
import re
import jinja2
import markdown

import render

class MainHandler(webapp2.RequestHandler):
    def get(self):
        self.response.write('wrong request')

    def post(self):

        baseHTML = self.request.get('base')
        md = self.request.get('slides')


	if not baseHTML or baseHTML == "" or not md or md == "":
            self.response.write('missing parameter')
            return

        template = jinja2.Template(baseHTML)
        md = md.replace('\r', '') 
        md_slides = md.split('\n---\n')

        outHTML = render.render_slides(template, md_slides)

        self.response.write(outHTML)

app = webapp2.WSGIApplication([
    ('/', MainHandler)
], debug=True)
