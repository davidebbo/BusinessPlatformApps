var gulp = require('gulp');
var gulp = require('gulp');
var bundler = require('aurelia-bundler');

//var notify = require('gulp-notify');
var typescript = require('gulp-typescript');
var babel = require('gulp-babel');
//var exec = require('child_process').exec;
//var htmlmin = require('gulp-htmlmin');

gulp.task('Setup', function () {
    return gulp.src('jspm_packages/**/*').pipe(gulp.dest('wwwroot/jspm_packages'));
});