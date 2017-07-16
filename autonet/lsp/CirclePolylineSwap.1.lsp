;;  CirclePolylineSwap.lsp [command names: C2P & P2C]
;;  Two commands, to convert in both directions between a Circle and a circular
;;  (two-equal-arc-segment closed) Polyline, such as the Donut command makes.
;;  Both commands:
;;  1. ask User to select again if they miss, pick an incorrect object type, or pick an
;;      object on a locked Layer;
;;  2. remove selected/converted object, but can easily be edited to retain it;
;;  3. account for different Coordinate Systems;
;;  4. retain non-default/non-Bylayer color, linetype, linetype scale, lineweight,
;;      and/or thickness.
;;  See additional notes above each command's definition.
;;  Kent Cooper, May 2011

;;  C2P
;;  To convert a selected Circle to a two-equal-arc-segment closed zero-
;;  width Polyline circle [Donut w/ equal inside & outside diameters],
;;  which can then be modified as desired [given width, etc.], since Pedit
;;  will not accept selection of a Circle.
;
(defun C:C2P (/ *error* cmde csel cir cdata cctr crad cextdir pdata)
  (vl-load-com)
  (defun *error* (errmsg)
    (if (not (wcmatch errmsg "Function cancelled,quit / exit abort,console break"))
      (princ (strcat "\nError: " errmsg))
    ); end if
    (command "_.undo" "_end")
    (setvar 'cmdecho cmde)
  ); end defun - *error*
  (setq cmde (getvar 'cmdecho))
  (setvar 'cmdecho 0)
  (command "_.undo" "_begin")
  (prompt "\nTo convert a Circle to its Polyline equivalent,")
  (while
    (not
      (and
        (setq csel (ssget ":S" '((0 . "CIRCLE"))))
        (= (cdr (assoc 70 (tblsearch "layer" (cdr (assoc 8 (entget (ssname csel 0))))))) 0)
          ; 0 for Unlocked, 4 for Locked
      ); end and
    ); end not
    (prompt "\nNothing selected, or not a Circle, or on a Locked Layer.")
  ); end while
  (setq
    cir (ssname csel 0); Circle entity name
    cdata (entget cir); entity data
    cctr (cdr (assoc 10 cdata)); center point, OCS for Circle & LWPolyline w/ WCS 0,0,0 as origin
    crad (cdr (assoc 40 cdata)); radius
    cextdir (assoc 210 cdata); extrusion direction
  ); end setq
  (setq 
    pdata (vl-remove-if-not '(lambda (x) (member (car x) '(67 410 8 62 6 48 370 39))) cdata)
      ; start Polyline entity data list -- remove Circle-specific entries from
      ; Circle's entity data, and extrusion direction; 62 Color, 6 Linetype, 48
      ; LTScale, 370 LWeight, 39 Thickness present only if not default/bylayer
    pdata
      (append ; add Polyline-specific entries
        '((0 . "LWPOLYLINE") (100 . "AcDbEntity"))
        pdata ; remaining non-entity-type-specific entries
        '((100 . "AcDbPolyline") (90 . 2) (70 . 129) (43 . 0.0))
          ; 90 = # of vertices, 70 1 bit = closed 128 bit = ltype gen. on, 43 = global width
        (list
          (cons 38 (caddr cctr)); elevation in OCS above WCS origin [Z of Circle center]
          (cons 10 (list (- (car cctr) crad) (cadr cctr))); vertex 1
          '(40 . 0.0) '(41 . 0.0) '(42 . 1); 0 width, semi-circle bulge factors
          (cons 10 (list (+ (car cctr) crad) (cadr cctr))); vertex 2
          '(40 . 0.0) '(41 . 0.0) '(42 . 1)
          cextdir ; extr. dir. at end [if in middle, reverts to (210 0.0 0.0 1.0) in (entmake)]
        ); end list
      ); end append & pdata
  ); end setq
  (entmake pdata)
  (entdel cir); [remove or comment out this line to retain selected Circle]
  (command "_.undo" "_end")
  (setvar 'cmdecho cmde)
  (princ)
); end defun

;;  P2C
;;  To convert a selected closed two-equal-arc-segment global-width circular
;;  Polyline [Donut] to a true Circle.  If selected Polyline has non-zero global
;;  width, offers User option to draw Circle along center-line or along inside or
;;  outside edge of width, and retains choice as default for next use.
;;  Works on both old-style "heavy" and newer "lightweight" Polylines.
;;  [Will not work on one with more than two segments, or with two unequal-
;;  included-angle segments, even if truly circular.]
;
(defun C:P2C (/ *error* cmde psel pl pdata pwidadj cposdef cpostemp pv1 pv2 cdata)
  (vl-load-com)
  (defun *error* (errmsg)
    (if (not (wcmatch errmsg "Function cancelled,quit / exit abort,console break"))
      (princ (strcat "\nError: " errmsg))
    ); end if
    (command "_.undo" "_end")
    (setvar 'cmdecho cmde)
  ); end defun - *error*
  (setq cmde (getvar 'cmdecho))
  (setvar 'cmdecho 0)
  (command "_.undo" "_begin")
  (prompt "\nTo convert a Polyline circle to a true Circle,")
  (while
    (not
      (and
        (setq psel (ssget ":S" '((0 . "*POLYLINE"))))
        (if psel (setq pl (ssname psel 0) pdata (entget pl)))
        (= (cdr (assoc 70 (tblsearch "layer" (cdr (assoc 8 pdata))))) 0)
          ; 0 for Unlocked, 4 for Locked
        (if (= (cdr (assoc 0 pdata)) "POLYLINE"); "heavy" Polyline
          (progn; then
            (command "_.convertpoly" "_light" pl ""); retains same entity name
            (setq pdata (entget pl)); replace "heavy" Polyline entity data
          ); end progn
          T; else - to not return nil for LWPolyline
        ); end if
        (member '(90 . 2) pdata); two vertices
        (member '(42 . 1.0) (cdr (member '(42 . 1.0) pdata))); two half-circle bulge factors
          ; needs to be really precise -- will be for one made with Donut,
          ; but may not be for one made with, for example, Pline [pt] Arc
          ; Direction [direction] [halfway around] Close, even with Snap on
      ); end and
    ); end not
    (prompt "\nNothing selected, or not a circular Polyline [Donut], or on a Locked Layer.")
  ); end while
  (if (and (assoc 43 pdata) (/= (cdr (assoc 43 pdata)) 0)); global non-zero width
    (progn; then
      (initget "Center Inside Outside")
      (setq
        pwidadj (/ (cdr (assoc 43 pdata)) 2)
        cposdef (cond (_P2Ccpos_) (T "Center")); Center default on first use
        cpostemp
          (getkword
            (strcat
              "\nCircle position on Donut [Center/Inside/Outside] <"
              (substr cposdef 1 1)
              ">: "
            ); end strcat
          ); end getkword & cpostemp
        _P2Ccpos_ (cond (cpostemp) (cposdef))
      ); end setq
    ); end progn
    (setq pwidadj 0); else
  ); end if
  (setq
    pv1 (cdr (assoc 10 pdata)); = Polyline Vertex 1 [XY]
    pv2 (cdr (assoc 10 (cdr (member (assoc 10 pdata) pdata)))); = Polyline Vertex 2 [XY]
      ; can't use parameter XYZ WCS locations, because cdata needs XY OCS locations
    cdata (vl-remove-if-not '(lambda (x) (member (car x) '(67 410 8 62 6 48 370 39))) pdata)
      ; build circle entity data list -- remove Polyline-specific entries from
      ; Polyline's entity data, and extrusion direction; 62 Color, 6 Linetype, 48
      ; LTScale, 370 LWeight, 39 Thickness present only if not default/bylayer
    cdata
      (append ; add circle-specific entries
        '((0 . "CIRCLE") (100 . "AcDbEntity"))
        cdata ; remaining non-entity-type-specific entries
        (list
          '(100 . "AcDbCircle")
          (list
            10 ; center
            (/ (+ (car pv1) (car pv2)) 2); X = halfway between X's of vertices
            (/ (+ (cadr pv1) (cadr pv2)) 2); Y = halfway between Y's of vertices
            (cdr (assoc 38 pdata))); Z of Circle = elevation of Pline
          (cons
            40 ; radius
            (+
              (/
                (distance; diameter -- needs 3D points for 3D distance
                  (vlax-curve-getStartPoint pl)
                  (vlax-curve-getPointAtParam pl 1)
                ); end distance
                2
              ); end /
              (cond
                ((= _P2Ccpos_ "Inside") (- pwidadj))
                ((= _P2Ccpos_ "Outside") pwidadj)
                (T 0); Pline with width & Center option, or no width/no position prompt
              ); end cond
            ); end +
          ); end cons
          (assoc 210 pdata); extr. dir. at end [if in middle, reverts to (210 0.0 0.0 1.0) in (entmake)]
        ); end list
      ); end append & cdata
  ); end setq
  (entmake cdata)
  (entdel pl)
    ; [remove or comment out above line to retain selected
    ; Polyline -- will be left lightweight if originally heavy]
  (command "_.undo" "_end")
  (setvar 'cmdecho cmde)
  (princ)
); end defun
(prompt "\nType C2P to convert a Circle to its Polyline equivalent.")
(prompt "\nType P2C to convert a circular Polyline to its Circle equivalent.")
