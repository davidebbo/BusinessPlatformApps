var gulp = require('gulp');
var gulp = require('gulp');
var bundler = require('aurelia-bundler');
var runSequence = require('run-sequence');

//var notify = require('gulp-notify');
var typescript = require('gulp-typescript');
var babel = require('gulp-babel');
var del = require('del');
//var exec = require('child_process').exec;
//var htmlmin = require('gulp-htmlmin');

var config = {
    force: true,
    baseURL: './wwwroot',
    configPath: './wwwroot/config.js',
    bundles: {
        "dist/app-build": {
            includes: [
              '*.js',
              '*.html!text',
              '*.css!text',
              'bootstrap/css/bootstrap.css!text'
            ],
            options: {
                inject: true,
                minify: true
            }
        },

        "dist/aurelia": {
            includes: [
                "aurelia-bootstrapper",
                "aurelia-event-aggregator",
                "aurelia-fetch-client",
                "aurelia-framework",
                "aurelia-history-browser",
                "aurelia-loader-default",
                "aurelia-logging-console",
                "aurelia-router",
                "aurelia-templating-binding",
                "aurelia-templating-resources",
                "aurelia-templating-router",
                "aurelia-http-client",
                "aurelia-polyfills"
            ],
           
            options: {
                "inject": true,
                "minify": true,
                "depCache": true
            }
        }
    }
};

gulp.task('Clean-Bundle', function () {
    return del([
    'wwwroot/dist/aurelia.js',
    'wwwroot/dist/app-build.js'
    ]);
});

gulp.task('Post-Build', function (callback) {
    runSequence(
        'unbundle',
        //'Clean-Bundle',
        //      'bundle',
              callback);
});

gulp.task('bundle', function () {
    return bundler.bundle(config);
});

gulp.task('unbundle', function () {
    return bundler.unbundle(config);
});