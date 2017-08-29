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
;;; Modified by igal Averbuh 2017
(defun C:C2P (/ *error* doc conv csel cir cdata cctr crad pdata)
  (defun *error* (errmsg)
    (if (not (wcmatch errmsg "Function cancelled,quit / exit abort,console break"))
      (princ (strcat "\nError: " errmsg))
    ); if
    (vla-endundomark doc)
  ); defun -- *error*

  (setq doc (vla-get-activedocument (vlax-get-acad-object)))
  (vla-startundomark doc)
  (setq conv 0)
  (prompt "\nTo convert Circle(s) to Polyline equivalent(s),")
  (if (setq csel (ssget "I" '((0 . "CIRCLE")))); User selection
    (progn
      (repeat (sslength csel); then
        (setq
          cir (ssname csel 0); Circle entity name
          cdata (entget cir); entity data
          cctr (cdr (assoc 10 cdata)); center point, OCS for Circle & LWPolyline w/ WCS 0,0,0 as origin
          crad (cdr (assoc 40 cdata)); radius
          pdata (vl-remove-if-not '(lambda (x) (member (car x) '(67 410 8 62 6 48 370 39))) cdata)
            ; start Polyline entity data list -- remove Circle-specific entries from
            ; Circle's entity data, and extrusion direction; 62 Color, 6 Linetype, 48
            ; LTScale, 370 LWeight, 39 Thickness present only if not default/bylayer
        ); setq
        (entmake
          (append ; add Polyline-specific entries
            '((0 . "LWPOLYLINE") (100 . "AcDbEntity"))
            pdata ; remaining non-entity-type-specific entries
            (list
              '(100 . "AcDbPolyline")
              '(90 . 2); # of vertices
              (cons 70 (1+ (* 128 (getvar 'plinegen)))); closed [the 1], and uses
                ; current linetype-generation setting; change above line to
                ; '(70 . 129) to force linetype generation on, '(70 . 1) to force it off
              '(43 . 0.0); global width
              (cons 38 (caddr cctr)); elevation in OCS above WCS origin [Z of Circle center]
              (cons 10 (list (- (car cctr) crad) (cadr cctr))); vertex 1
              '(40 . 0.0) '(41 . 0.0) '(42 . 1); 0 start & end widths, semi-circle bulge factor
              (cons 10 (list (+ (car cctr) crad) (cadr cctr))); vertex 2
              '(40 . 0.0) '(41 . 0.0) '(42 . 1)
              (assoc 210 cdata) ; extr. dir. at end [if in middle, reverts to (210 0.0 0.0 1.0) in (entmake)]
            ); list
          ); append
        ); entmake
        ;(ssdel cir csel)
        (entdel cir); [remove or comment out this line to retain selected Circle(s)]
        (setq conv (1+ conv))
      ); repeat -- then
      (prompt (strcat "\n" (itoa conv) " Circle(s) converted to Polyline(s)."))
    ); progn -- then
    (prompt "\nNo Circle(s) found [on unlocked Layer(s)]."); else
  ); if
  (vla-endundomark doc)
  (princ)
); defun


(vlax-remove-cmd "ctop")
(vlax-add-cmd "ctop" 'C2P  "ctop"  ACRX_CMD_USEPICKSET+ACRX_CMD_REDRAW)

;;  P2C
;;  To convert selected closed all-arc-segment circular Polylines [e.g. Donuts, or
;;  others if truly circular, including with more than two segments, of unequal
;;  included angles, of varying widths] to true Circles.  If any Polylines within
;;  single selection set have non-zero global width, offers User option to draw
;;  Circle along center-line or along inside or outside edge of width; applies
;;  same position to all in selection, and retains choice for subsequent default.
;;  Works on both old-style "heavy" 2D and newer "lightweight" Polylines.
;
(defun C:P2C
  (/ *error* ptpar doc cmde conv notconv psel pl pdata ctr radlist ver plhw cpos pwadj cdata)

  (defun *error* (errmsg)
    (if (not (wcmatch errmsg "Function cancelled,quit / exit abort,console break"))
      (princ (strcat "\nError: " errmsg))
    ); if
    (vla-endundomark doc)
    (setvar 'cmdecho cmde)
  ); defun -- *error*

  (defun ptpar (par); PoinT on polyline at specified PARameter value
    (vlax-curve-getPointAtParam pl par)
  ); defun - ptpar

  (setq doc (vla-get-activedocument (vlax-get-acad-object)))
  (vla-startundomark doc)
  (setq
    cmde (getvar 'cmdecho); for possible ConvertPoly command later
    conv 0
    notconv 0
  ); setq
  (prompt "\nTo convert Polyline circle(s) to true Circle(s),")
  (if (setq psel (ssget "I" '((0 . "*POLYLINE")))); User selection
    (repeat (sslength psel); then
      (setq pl (ssname psel 0)); first Polyline entity name
      (if
        (and
          (vlax-curve-isClosed pl)
          (setq pdata (entget pl))
          (/= (cdr (assoc 100 (reverse pdata))) "AcDb3dPolyline"); not 3D
        ); and
        (progn ; then -- closed LW/2D Polyline
          (if (= (cdr (assoc 100 (reverse pdata))) "AcDb2dPolyline"); "heavy" 2D
            (progn; then
              (setvar 'cmdecho 0)
              (command "_.convertpoly" "_light" pl ""); retains same entity name
              (setvar 'cmdecho cmde)
              (setq pdata (entget pl)); replace "heavy" Polyline entity data
            ); progn
          ); if -- 2D
          (if
            (and
              (not (member '(42 . 0.0) pdata)); no line segments
              (apply '= ; all arc segments bulge in same direction [no retracing]
                (mapcar 'minusp
                  (mapcar 'cdr (vl-remove-if-not '(lambda (x) (= (car x) 42)) pdata))
                ); mapcar
              ); apply
            ); and
            (progn ; then -- check for circularity
              (setq
                ctr
                  (mapcar '/
                    (mapcar '+
                      (vlax-curve-getStartPoint pl)
                      (vlax-curve-getPointAtDist ; point half-way around
                        pl
                        (/
                          (vlax-curve-getDistAtParam pl (vlax-curve-getEndParam pl))
                          2.0
                        ); /
                      ); getPointAtDist
                    ); mapcar +
                    '(2 2 2)
                  ); mapcar \ & ctr
                radlist (list (distance ctr (vlax-curve-getStartPoint pl)))
                ver 0
              ); setq
              (repeat (* (cdr (assoc 90 pdata)) 2); check distance from ctr at all vertices & midpoints
                (setq rad (distance ctr (ptpar (setq ver (+ ver 0.5))))); [depends on not being "quirky"]
                (if (not (equal rad (car radlist) 1e-6)); different radius?
                  (setq radlist (cons rad radlist)); then -- add to list
                ); if
              ); repeat
              (if (= (length radlist) 1); all distances from center the same = circular -- convert it
                (progn ; then
                  (if (assoc 43 pdata); has global width
                    (if ; outer then
                      (and
                        (/= (setq plhw (/ (cdr (assoc 43 pdata)) 2)) 0); PolyLine Half-Width not zero
                        (not cpos); not established yet -- ask only once per selection set
                      ); and
                      (progn ; inner then -- get Circle-position option
                        (initget "Center Inside Outside")
                        (setq
                          _P2Ccpos_ ; _global_ variable
                            (cond
                              ((getkword
                                (strcat ; will apply same User choice to ALL conversions in selection set
                                  "\nCircle position on Donut(s) [Center/Inside/Outside] <"
                                  (substr (cond (_P2Ccpos_) ("C")) 1 1); Center default on first use
                                  ">: "
                                ); strcat
                              )); getkword & first condition
                              (_P2Ccpos_); retain on Enter if default established
                              ("Center"); default on first use
                            ); cond & _P2Ccpos_
                          cpos T ; option established for subsequent Polylines this selection
                        ); setq
                      ); progn -- inner then -- wide global-width Polyline -- Circle placement option
                    ); if -- outer then [global width]
                    (setq plhw 0); outer else [has varying widths -- use Center]
                  ); if [global width or not]
                  (setq pwadj ; = Polyline Width ADJustment
                    (cond ; then -- first two equivalent to "Center" for zero-width or varying-width originals
                      ((= _P2Ccpos_ "Outside") plhw)
                      ((= _P2Ccpos_ "Inside") (- plhw))
                      (0); Center option or no wide global-width originals yet
                    ); cond
                  ); setq
                  (setq cdata (vl-remove-if-not '(lambda (x) (member (car x) '(67 410 8 62 6 48 370 39))) pdata))
                    ; build circle entity data list -- remove Polyline-specific entries from
                    ; Polyline's entity data, and extrusion direction; 62 Color, 6 Linetype, 48
                    ; LTScale, 370 LWeight present only if not default/bylayer; 39 Thickness
                  (entmake
                    (append ; add circle-specific entries
                      '((0 . "CIRCLE") (100 . "AcDbEntity"))
                      cdata ; remaining non-entity-type-specific entries
                      (list
                        '(100 . "AcDbCircle")
                        (cons 10 (trans ctr 0 pl)); center -- WCS to OCS
                        (cons 40 (+ (car radlist) pwadj)); radius
                        (assoc 210 pdata); extr. dir. at end [if in middle, reverts to (210 0.0 0.0 1.0) in (entmake)]
                      ); list
                    ); append
                  ); entmake
                  (entdel pl)
                    ; [remove or comment out above line to retain selected
                    ; Polyline -- will be left lightweight if originally heavy]
                  (setq conv (1+ conv))
                ); progn -- then
                (setq notconv (1+ notconv)); else -- not circular
              ); if -- circular
            ); progn -- then
            (setq notconv (1+ notconv)); else -- has line segment(s) or different-direction bulge(s)
          ); if -- no line segments, same bulge directions
        ); progn -- then
        (setq notconv (1+ notconv)); else -- not closed or LW/2D
      ); if -- closed LW/2D
      (ssdel pl psel)
    ); repeat -- then
    (prompt "\nNo Polyline(s) found [on unlocked Layer(s)]."); else
  ); if -- User selection
  (prompt
    (strcat
      "\n"
      (if (> conv 0) (itoa conv) "No")
      " Polyline(s) converted to Circle(s). "
      (if (> notconv 0)
        (strcat (itoa notconv) " Polyline(s) not circular and/or not closed.")
        ""
      ); if
    ); strcat
  ); prompt
  (vla-endundomark doc)
  (princ)
); defun

(vl-load-com)
(prompt "\nType C2P to convert Circle(s) to equivalent Polyline(s).")
(prompt "\nType P2C to convert circular Polyline(s) to equivalent Circle(s).")
