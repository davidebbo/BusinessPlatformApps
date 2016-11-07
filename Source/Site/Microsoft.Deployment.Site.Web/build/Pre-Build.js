var gulp = require('gulp');

var typescript = require('gulp-typescript');

var babel = require('gulp-babel');

var runSequence = require('run-sequence');

var del = require('del');

gulp.task('Copy-Build-App', function () {
    return gulp.src('../../Apps/**/Web/**/*').pipe(gulp.dest('wwwroot/dist/Apps/'));
});

gulp.task('Copy-Build-SiteCommon', function () {
    return gulp.src('../../SiteCommon/Web/**/*').pipe(gulp.dest('wwwroot/dist/SiteCommon/Web'));
});

gulp.task('Copy-Build-Src', function () {
    return gulp.src('src/**/*').pipe(gulp.dest('wwwroot/dist/'));
});

gulp.task('CleanDirectory', function () {
    return del(['wwwroot/dist']);
});

gulp.task('Pre-Build', function (callback) {
    runSequence('CleanDirectory', ['Copy-Build-App', 'Copy-Build-Src', 'Copy-Build-SiteCommon'], callback);
});