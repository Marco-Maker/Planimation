; The elevator moves trough the floors with processes and events

(define (domain elevator)
  (:requirements :strips :typing)
  (:types
    person elevator
  )
  (:predicates
    (reached ?p - person)
    (in ?e - elevator ?p - person)
    (is-moving-up ?e - elevator)
    (is-moving ?e - elevator)
    (is-moving-down ?e - elevator)
  )
  (:functions
    (at-elevator ?e - elevator)
    (at-person ?p - person)
    (target ?p - person)
    (weight ?p - person)
    (capacity ?e - elevator)
    (max-load ?e - elevator)
    (passengers ?e - elevator)
    (load ?e - elevator)
    (moving-percentage ?e - elevator)
    (floors)
  )

  ; (:durative-action move-up
  ;   :parameters (?e - elevator)
  ;   :duration (= ?duration 10)
  ;   :condition (and
  ;     (at start (< (at-elevator ?e) (floors)))
  ;   )
  ;   :effect (and
  ;     (at end (increase (at-elevator ?e) 1))
  ;   )
  ; )

  (:action start-move-up
    :parameters (?e - elevator)
    :precondition (and
      (not (is-moving ?e))
      (< (at-elevator ?e) (floors))
    )
    :effect (and
      (is-moving-up ?e)
      (is-moving ?e)
      (assign (moving-percentage ?e) 0)
    )
  )

  (:event end-move-up
    :parameters (?e - elevator)
    :precondition (and
      (is-moving-up ?e)
      (>= (moving-percentage ?e) 100)
    )
    :effect (and
      (increase (at-elevator ?e) 1)
      (not (is-moving ?e))
      (not (is-moving-up ?e))
    )
  )

  ; (:durative-action move-down
  ;   :parameters (?e - elevator)
  ;   :duration (= ?duration 10)
  ;   :condition (and
  ;     (at start (> (at-elevator ?e) 0))
  ;   )
  ;   :effect (and
  ;     (at end (decrease (at-elevator ?e) 1))
  ;   )
  ; )

  (:action start-move-down
    :parameters (?e - elevator)
    :precondition (and
      (not (is-moving ?e))
      (> (at-elevator ?e) 0)
    )
    :effect (and
      (is-moving-down ?e)
      (is-moving ?e)
      (assign (moving-percentage ?e) 0)
    )
  )

  (:event end-move-down
    :parameters (?e - elevator)
    :precondition (and
      (is-moving-down ?e)
      (>= (moving-percentage ?e) 100)
    )
    :effect (and
      (decrease (at-elevator ?e) 1)
      (not (is-moving ?e))
      (not (is-moving-down ?e))
    )
  )

  (:process is-moving
    :parameters (?e - elevator)
    :precondition (and
      (is-moving ?e)
    )
    :effect (and
      (increase
        (moving-percentage ?e)
        (* #t 10) ; #EASIER
        ; (* #t (- 10 (* 3 (/ (load ?e) (max-load ?e))))) ; percentage + {10 - [3 *  (load / max-load)]} ; #MUCH SLOWER
;(* #t (* 0.1 (- 110 (moving-percentage ?e)))) ;# NON LINEAR
        )
    )
  )

  (:action load
    :parameters (?p - person ?e - elevator)
    :precondition (and
      (>= (at-person ?p) (at-elevator ?e))
      (<= (+ (passengers ?e) 1) (capacity ?e))
      (<= (+ (load ?e) (weight ?p)) (max-load ?e)) ;Now the weight is restricted
    )
    :effect (and
      (in ?e ?p)
      (assign (at-person ?p) -1)
      (increase (passengers ?e) 1)
      (increase (load ?e) (weight ?p)) ;The weight is added when a passengers enters 
    )
  )

  (:action unload
    :parameters (?p - person ?e - elevator)
    :precondition (and
      (in ?e ?p)
    )
    :effect (and
      (not (in ?e ?p))
      (assign (at-person ?p) (at-elevator ?e))
      (decrease (passengers ?e) 1)
      (decrease (load ?e) (weight ?p)) ;The weight is removed when a passengers exits 
    )
  )

  (:action reached
    :parameters (?p - person)
    :precondition (and
      (= (at-person ?p) (target ?p))
      (not (reached ?p))
    )
    :effect (and
      (reached ?p)
    )
  )

)
